using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByCorreoAsync(string correo);
    Task<Usuario?> GetByCodigoSistemaAsync(string codigoSistema);
    Task<IReadOnlyList<Usuario>> GetUsuariosPorRolAsync(int rolId);
    Task<IReadOnlyList<Usuario>> GetUsuariosPorAreaAsync(int areaId);
    Task<IReadOnlyList<Usuario>> GetAllWithIncludesAsync();
    Task<Usuario?> GetByIdWithIncludesAsync(int id);
}
