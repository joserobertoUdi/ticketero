using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface IPuestoRepository : IGenericRepository<Puesto>
{
    Task<IReadOnlyList<Puesto>> GetPuestosPorAreaAsync(int areaId);
}
