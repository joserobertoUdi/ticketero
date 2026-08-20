using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public class AbrirSesionUseCase : IAbrirSesionUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public AbrirSesionUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SesionDto> EjecutarAsync(AbrirSesionRequest request)
    {
        if (request.PuestoId <= 0)
            throw new InvalidOperationException("Debe seleccionar un puesto válido");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var puestoExiste = await _unitOfWork.Puestos.GetByIdAsync(request.PuestoId);
            if (puestoExiste == null || !puestoExiste.Estado)
                throw new InvalidOperationException("El puesto seleccionado no existe o no está disponible");

            var sesionActiva = await _unitOfWork.SesionesOperador.GetSesionActivaPorUsuarioAsync(request.UsuarioId);

            // Regla de tiempo máximo: cualquier atención activa que supere los
            // N minutos se finaliza automáticamente en todo el flujo, incluso al
            // reabrir sesión tras un cierre de pestaña.
            await _unitOfWork.FinalizarAtencionesVencidasAsync(ReglasAtencion.MaximoMinutosAtencion);

            // Caso 3: recuperación de tickets huérfanos. Si el operador dejó
            // atenciones activas (cierre de ventana sin evento, crash, etc.),
            // se finalizan y la sesión previa se cierra para no bloquear la cola.
            if (sesionActiva != null)
            {
                var atencionesActivas = await _unitOfWork.Atenciones.FindAsync(a =>
                    a.SesionOperadorId == sesionActiva.Id && a.FechaFin == null);

                foreach (var atencion in atencionesActivas)
                {
                    atencion.FechaFin = DateTime.UtcNow;
                    atencion.TiempoAtencionSegundos = (int)(atencion.FechaFin.Value - atencion.FechaInicio).TotalSeconds;
                    atencion.EstadoTicketId = TicketEstado.Cerrado;
                    atencion.Observacion = atencion.Observacion ?? "Atención finalizada por sesión reemplazada";

                    var ticket = await _unitOfWork.Tickets.GetByIdAsync(atencion.TicketId);
                    if (ticket != null)
                    {
                        ticket.EstadoTicketId = TicketEstado.Cerrado;
                        ticket.FechaCierre = ticket.FechaCierre ?? DateTime.UtcNow;
                    }
                }

                sesionActiva.FechaFin = DateTime.UtcNow;
            }

            var sesion = new SesionOperador
            {
                UsuarioId = request.UsuarioId,
                PuestoId = request.PuestoId,
                FechaInicio = DateTime.UtcNow
            };

            await _unitOfWork.SesionesOperador.AddAsync(sesion);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return new SesionDto
            {
                SesionOperadorId = sesion.Id,
                FechaInicio = sesion.FechaInicio,
                EstaActiva = true,
                AtencionesFinalizadas = 0
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}