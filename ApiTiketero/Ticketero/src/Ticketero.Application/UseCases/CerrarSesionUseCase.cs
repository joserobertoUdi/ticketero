using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public interface ICerrarSesionUseCase
{
    Task<SesionDto> EjecutarAsync(int sesionOperadorId, string? motivo = null);
}

public class CerrarSesionUseCase : ICerrarSesionUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public CerrarSesionUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SesionDto> EjecutarAsync(int sesionOperadorId, string? motivo = null)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var sesion = await _unitOfWork.SesionesOperador.GetByIdAsync(sesionOperadorId)
                ?? throw new InvalidOperationException("Sesión no encontrada");

            if (sesion.FechaFin != null)
                throw new InvalidOperationException("La sesión ya está cerrada");

            // Regla de tiempo máximo: cualquier atención activa que supere los
            // N minutos se finaliza automáticamente antes de procesar el cierre.
            await _unitOfWork.FinalizarAtencionesVencidasAsync(ReglasAtencion.MaximoMinutosAtencion);

            // Caso 3: al cerrar sesión se finalizan las atenciones activas.
            // Esto evita tickets "a la deriva" cuando el operador cierra la
            // ventana mientras atiende (el ticket queda finalizado y la cola
            // no queda bloqueada por una sesión abierta sin operador).
            var atencionesActivas = await _unitOfWork.Atenciones.FindAsync(a =>
                a.SesionOperadorId == sesionOperadorId && a.FechaFin == null);

            foreach (var atencion in atencionesActivas)
            {
                atencion.FechaFin = DateTime.UtcNow;
                atencion.TiempoAtencionSegundos = (int)(atencion.FechaFin.Value - atencion.FechaInicio).TotalSeconds;
                atencion.EstadoTicketId = TicketEstado.Cerrado;
                atencion.Observacion = atencion.Observacion ?? motivo ?? "Atención finalizada al cerrar sesión";

                var ticket = await _unitOfWork.Tickets.GetByIdAsync(atencion.TicketId);
                if (ticket != null)
                {
                    ticket.EstadoTicketId = TicketEstado.Cerrado;
                    ticket.FechaCierre = ticket.FechaCierre ?? DateTime.UtcNow;
                }
            }

            // Gap 13: Solo actualizar FechaFin.
            // En la BD, Estado tiene DEFAULT(1) y es flag de existencia lógica del registro.
            // La sesión activa se determina por: WHERE FechaFin IS NULL
            // (ver UX_SesionOperador_Usuario_Activa en SesionOperador_Tabla.sql)
            sesion.FechaFin = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return new SesionDto
            {
                SesionOperadorId = sesion.Id,
                FechaInicio = sesion.FechaInicio,
                FechaFin = sesion.FechaFin,
                EstaActiva = false,
                AtencionesFinalizadas = atencionesActivas.Count
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}