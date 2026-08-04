using Ticketero.Application.DTOs;

namespace Ticketero.Application.UseCases;

public interface ICerrarTicketUseCase
{
    Task<AtenderTicketResponse> EjecutarAsync(CerrarTicketRequest request);
}
