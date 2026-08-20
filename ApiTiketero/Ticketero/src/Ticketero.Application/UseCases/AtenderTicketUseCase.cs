using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public class AtenderTicketUseCase : IAtenderTicketUseCase
{
    private static readonly int[] EstadosAtendibles = [TicketEstado.Asignado, TicketEstado.Nuevo];
    private readonly IUnitOfWork _unitOfWork;

    public AtenderTicketUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AtenderTicketResponse> EjecutarAsync(AtenderTicketRequest request)
    {
        await _unitOfWork.FinalizarAtencionesVencidasAsync(ReglasAtencion.MaximoMinutosAtencion);

        var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId)
            ?? throw new InvalidOperationException($"Ticket {request.TicketId} no encontrado");

        if (!EstadosAtendibles.Contains(ticket.EstadoTicketId))
            throw new InvalidOperationException(
                $"No se puede iniciar atención del ticket {ticket.NumeroTicket}. Estado actual (Id={ticket.EstadoTicketId}) no válido. El ticket debe estar en estado Llamado/Asignado o Nuevo.");

        // Caso 1: si el ticket ya está siendo atendido por otro operador,
        // el segundo debe avanzar al siguiente ticket (el controlador lo resuelve).
        var atencionActiva = await _unitOfWork.Atenciones.GetAtencionActivaPorTicketAsync(request.TicketId);
        if (atencionActiva != null)
        {
            if (atencionActiva.UsuarioId != request.UsuarioId)
                throw new InvalidOperationException(
                    $"El ticket {ticket.NumeroTicket} ya está siendo atendido por otro operador.");

            // Reintento idempotente del mismo operador.
            return new AtenderTicketResponse
            {
                AtencionId = atencionActiva.Id,
                TicketId = ticket.Id,
                Mensaje = "Atención ya iniciada por el mismo operador"
            };
        }

        var perteneceArea = (await _unitOfWork.UsuariosArea.FindAsync(
            ua => ua.UsuarioId == request.UsuarioId && ua.AreaId == request.AreaId))
            .Any();
        if (!perteneceArea)
            throw new InvalidOperationException(
                $"El usuario {request.UsuarioId} no pertenece al área {request.AreaId} y no puede atender tickets en ella.");

        var sesion = await _unitOfWork.SesionesOperador.GetByIdAsync(request.SesionOperadorId)
            ?? throw new InvalidOperationException("Sesión de operador no encontrada");

        var atencion = new Atencion
        {
            TicketId = request.TicketId,
            UsuarioId = request.UsuarioId,
            AreaId = request.AreaId,
            ServicioId = request.ServicioId,
            EstadoTicketId = TicketEstado.EnAtencion,
            SesionOperadorId = request.SesionOperadorId,
            FechaInicio = DateTime.UtcNow
        };

        ticket.EstadoTicketId = TicketEstado.EnAtencion;

        await _unitOfWork.Atenciones.AddAsync(atencion);
        await _unitOfWork.SaveChangesAsync();

        return new AtenderTicketResponse
        {
            AtencionId = atencion.Id,
            TicketId = request.TicketId,
            Mensaje = "Atención iniciada correctamente"
        };
    }
}

