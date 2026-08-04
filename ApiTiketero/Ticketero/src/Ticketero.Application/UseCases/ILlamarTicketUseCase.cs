using Ticketero.Application.DTOs;

namespace Ticketero.Application.UseCases;

public interface ILlamarTicketUseCase
{
    Task<AtenderTicketResponse> EjecutarAsync(int ticketId, int usuarioId, int kioskoId);
}
