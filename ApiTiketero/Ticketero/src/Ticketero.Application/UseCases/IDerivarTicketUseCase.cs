using Ticketero.Application.DTOs;

namespace Ticketero.Application.UseCases;

public interface IDerivarTicketUseCase
{
    Task<AtenderTicketResponse> EjecutarAsync(DerivarTicketRequest request);
}
