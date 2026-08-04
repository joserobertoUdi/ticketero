using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<Ticket?> GetByNumeroTicketAsync(string numeroTicket);
    Task<IReadOnlyList<Ticket>> GetTicketsPorAreaAsync(int areaId);
    Task<IReadOnlyList<Ticket>> GetTicketsPorEstadoAsync(int estadoTicketId);
    Task<IReadOnlyList<Ticket>> GetTicketsPendientesAsync();
    Task<IReadOnlyList<Ticket>> GetTicketsPendientesPorAreaAsync(int areaId);
    Task<Ticket?> GetByIdWithIncludesAsync(int id);
    Task<IReadOnlyList<Ticket>> GetTicketsByDateRangeAsync(DateTime inicio, DateTime fin);
    Task<Ticket?> GetCurrentTicketByUserAsync(int usuarioId);
    Task<IReadOnlyList<Ticket>> GetHistoryAsync(int usuarioId, DateTime fecha);
}
