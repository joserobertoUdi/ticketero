using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface IMarcacionRepository : IGenericRepository<Marcacion>
{
    Task<IReadOnlyList<Marcacion>> GetMarcacionesPorTicketAsync(int ticketId);
    Task<byte> GetUltimoNumeroLlamadoAsync(int ticketId);
}
