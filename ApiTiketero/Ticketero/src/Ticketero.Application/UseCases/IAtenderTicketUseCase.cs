using Ticketero.Application.DTOs;

namespace Ticketero.Application.UseCases;

public interface IAtenderTicketUseCase
{
    Task<AtenderTicketResponse> EjecutarAsync(AtenderTicketRequest request);
}
