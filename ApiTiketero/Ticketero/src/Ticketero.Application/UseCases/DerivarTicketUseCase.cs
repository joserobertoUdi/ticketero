using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public class DerivarTicketUseCase : IDerivarTicketUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public DerivarTicketUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AtenderTicketResponse> EjecutarAsync(DerivarTicketRequest request)
    {
        if (request.AreaDestinoId <= 0)
            throw new InvalidOperationException("El área de destino no es válida");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.FinalizarAtencionesVencidasAsync(ReglasAtencion.MaximoMinutosAtencion);

            var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId)
                ?? throw new InvalidOperationException($"Ticket {request.TicketId} no encontrado");

            if (ticket.AreaActualId == request.AreaDestinoId)
                throw new InvalidOperationException("El ticket ya se encuentra en el área de destino");

            var areaDestino = await _unitOfWork.Areas.GetByIdAsync(request.AreaDestinoId);
            if (areaDestino == null)
                throw new InvalidOperationException("El área de destino no existe");

            var atencion = await _unitOfWork.Atenciones.GetByIdAsync(request.AtencionId)
                ?? throw new InvalidOperationException($"Atención {request.AtencionId} no encontrada");

            var perteneceArea = (await _unitOfWork.UsuariosArea.FindAsync(
                ua => ua.UsuarioId == atencion.UsuarioId && ua.AreaId == atencion.AreaId))
                .Any();
            if (!perteneceArea)
                throw new InvalidOperationException(
                    $"El usuario {atencion.UsuarioId} no pertenece al área {atencion.AreaId} y no puede derivar tickets desde ella.");

            atencion.FechaFin = DateTime.UtcNow;
            atencion.FueDerivado = true;
            atencion.AreaDestinoId = request.AreaDestinoId;
            atencion.Observacion = request.Observacion;
            atencion.TiempoAtencionSegundos = (int)(DateTime.UtcNow - atencion.FechaInicio).TotalSeconds;
            atencion.EstadoTicketId = TicketEstado.Cerrado;

            ticket.AreaActualId = request.AreaDestinoId;
            // Caso 2: el ticket derivado vuelve a estado Nuevo para reingresar
            // a la cola de llamada del área de destino.
            ticket.EstadoTicketId = TicketEstado.Nuevo;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return new AtenderTicketResponse
            {
                AtencionId = atencion.Id,
                Mensaje = "Ticket derivado exitosamente"
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
