using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface IAreaRepository : IGenericRepository<Area>
{
    Task<Area?> GetByPrefijoAsync(string prefijo);
}
