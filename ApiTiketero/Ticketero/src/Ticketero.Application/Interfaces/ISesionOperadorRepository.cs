using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface ISesionOperadorRepository : IGenericRepository<SesionOperador>
{
    Task<SesionOperador?> GetSesionActivaPorUsuarioAsync(int usuarioId);
    Task<SesionOperador?> GetSesionActivaPorPuestoAsync(int puestoId);
}
