using Ticketero.Application.DTOs;

namespace Ticketero.Application.UseCases;

public interface ICrearTicketUseCase
{
    Task<CreateTicketResponse> EjecutarAsync(CreateTicketRequest request);
}
