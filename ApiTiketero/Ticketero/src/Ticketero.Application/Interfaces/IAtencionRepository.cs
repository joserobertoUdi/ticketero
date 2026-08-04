using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface IAtencionRepository : IGenericRepository<Atencion>
{
    Task<IReadOnlyList<Atencion>> GetAtencionesPorTicketAsync(int ticketId);
    Task<IReadOnlyList<Atencion>> GetAtencionesPorUsuarioAsync(int usuarioId);
    Task<Atencion?> GetAtencionActivaPorTicketAsync(int ticketId);
}
