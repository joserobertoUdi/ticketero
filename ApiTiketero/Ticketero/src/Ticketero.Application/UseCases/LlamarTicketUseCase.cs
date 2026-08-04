using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public class LlamarTicketUseCase : ILlamarTicketUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public LlamarTicketUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AtenderTicketResponse> EjecutarAsync(int ticketId, int usuarioId, int kioskoId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId)
            ?? throw new InvalidOperationException($"Ticket {ticketId} no encontrado");

        if (!TicketEstado.EsEstadoLlamable(ticket.EstadoTicketId))
            throw new InvalidOperationException(
                $"No se puede llamar el ticket {ticket.NumeroTicket} porque su estado actual (Id={ticket.EstadoTicketId}) no permite ser llamado. Solo tickets Nuevos o En Espera pueden ser llamados.");

        var ultimoLlamado = await _unitOfWork.Marcaciones.GetUltimoNumeroLlamadoAsync(ticketId);
        var nuevoNumero = (byte)(ultimoLlamado < 254 ? ultimoLlamado + 1 : 1);

        ticket.EstadoTicketId = TicketEstado.Asignado;

        var kioskoFinalId = kioskoId;
        if (kioskoFinalId <= 0)
        {
            var activos = await _unitOfWork.Kioskos.FindAsync(k => k.Estado);
            kioskoFinalId = activos.FirstOrDefault()?.Id ?? 1;
        }
        var marcacion = new Marcacion
        {
            TicketId = ticketId,
            UsuarioId = usuarioId,
            KioskoId = kioskoFinalId,
            NumeroLlamado = nuevoNumero,
            FechaMarcacion = DateTime.UtcNow
        };

        await _unitOfWork.Marcaciones.AddAsync(marcacion);
        await _unitOfWork.SaveChangesAsync();

        return new AtenderTicketResponse
        {
            AtencionId = marcacion.Id,
            Mensaje = $"Llamado #{nuevoNumero} realizado para ticket {ticket.NumeroTicket}"
        };
    }
}

