using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public class CerrarTicketUseCase : ICerrarTicketUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public CerrarTicketUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AtenderTicketResponse> EjecutarAsync(CerrarTicketRequest request)
    {
        await _unitOfWork.FinalizarAtencionesVencidasAsync(ReglasAtencion.MaximoMinutosAtencion);

        var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId)
            ?? throw new InvalidOperationException($"Ticket {request.TicketId} no encontrado");

        var atencion = await _unitOfWork.Atenciones.GetByIdAsync(request.AtencionId)
            ?? throw new InvalidOperationException($"Atención {request.AtencionId} no encontrada");

        atencion.FechaFin = DateTime.UtcNow;
        atencion.TiempoAtencionSegundos = (int)(DateTime.UtcNow - atencion.FechaInicio).TotalSeconds;
        atencion.EstadoTicketId = TicketEstado.Cerrado;
        atencion.Observacion = request.Observacion;

        ticket.EstadoTicketId = TicketEstado.Cerrado;
        ticket.FechaCierre = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new AtenderTicketResponse
        {
            AtencionId = atencion.Id,
            Mensaje = "Ticket cerrado exitosamente"
        };
    }
}
