using Microsoft.EntityFrameworkCore;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Infrastructure.Repositories;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(DbContext context) : base(context) { }

    private IQueryable<Ticket> IncludeAll()
    {
        return _dbSet
            .Include(t => t.Servicio)
            .Include(t => t.TipoTicket)
            .Include(t => t.Prioridad)
            .Include(t => t.EstadoTicket)
            .Include(t => t.AreaActual)
            .Include(t => t.Atenciones)!.ThenInclude(a => a.Usuario)
            .Include(t => t.Atenciones)!.ThenInclude(a => a.Area)
            .Include(t => t.Atenciones)!.ThenInclude(a => a.AreaDestino)
            .Include(t => t.Marcaciones)!.ThenInclude(m => m.Usuario)
            .Include(t => t.Marcaciones)!.ThenInclude(m => m.Kiosko);
    }

    private IQueryable<Ticket> IncludeAllNoTracking()
    {
        return IncludeAll().AsNoTracking();
    }

    public async Task<Ticket?> GetByNumeroTicketAsync(string numeroTicket)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.NumeroTicket == numeroTicket);
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsPorAreaAsync(int areaId)
    {
        return await IncludeAllNoTracking().Where(t => t.AreaActualId == areaId).ToListAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsPendientesPorAreaAsync(int areaId)
    {
        return await IncludeAllNoTracking()
            .Where(t => t.AreaActualId == areaId
                && (t.EstadoTicketId == TicketEstado.Nuevo || t.EstadoTicketId == TicketEstado.EnEspera))
            .OrderBy(t => t.Prioridad!.Nivel)
            .ThenBy(t => t.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsPorEstadoAsync(int estadoTicketId)
    {
        return await _dbSet.Where(t => t.EstadoTicketId == estadoTicketId).ToListAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsPendientesAsync()
    {
        return await IncludeAllNoTracking()
            .Where(t => t.EstadoTicketId == TicketEstado.Nuevo || t.EstadoTicketId == TicketEstado.EnEspera)
            .OrderBy(t => t.Prioridad!.Nivel)
            .ThenBy(t => t.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Ticket?> GetByIdWithIncludesAsync(int id)
    {
        return await IncludeAll()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsByDateRangeAsync(DateTime inicio, DateTime fin)
    {
        return await IncludeAllNoTracking()
            .Where(t => t.FechaCreacion >= inicio && t.FechaCreacion < fin)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Ticket?> GetCurrentTicketByUserAsync(int usuarioId)
    {
        return await IncludeAll()
            .Where(t => t.Atenciones!.Any(a => a.UsuarioId == usuarioId) && t.EstadoTicketId != 6 && t.EstadoTicketId != 7)
            .OrderByDescending(t => t.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetHistoryAsync(int usuarioId, DateTime fecha)
    {
        var inicio = fecha.Date;
        var fin = inicio.AddDays(1);
        return await IncludeAll()
            .Where(t => t.Atenciones!.Any(a => a.UsuarioId == usuarioId)
                && t.FechaCreacion >= inicio && t.FechaCreacion < fin)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();
    }
}

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(DbContext context) : base(context) { }

    private IQueryable<Usuario> IncludeAll()
    {
        return _dbSet
            .Include(u => u.Rol)
            .Include(u => u.UsuariosArea)!.ThenInclude(ua => ua.Area);
    }

    public async Task<Usuario?> GetByCorreoAsync(string correo)
    {
        return await IncludeAll().FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task<Usuario?> GetByCodigoSistemaAsync(string codigoSistema)
    {
        return await IncludeAll().FirstOrDefaultAsync(u => u.CodigoSistema == codigoSistema);
    }

    public async Task<IReadOnlyList<Usuario>> GetUsuariosPorRolAsync(int rolId)
    {
        return await IncludeAll().Where(u => u.RolId == rolId && u.Estado).ToListAsync();
    }

    public async Task<IReadOnlyList<Usuario>> GetUsuariosPorAreaAsync(int areaId)
    {
        return await _context.Set<UsuarioArea>()
            .Where(ua => ua.AreaId == areaId && ua.Usuario!.Estado)
            .Select(ua => ua.Usuario!)
            .Include(u => u.Rol)
            .Include(u => u.UsuariosArea)!.ThenInclude(ua => ua.Area)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Usuario>> GetAllWithIncludesAsync()
    {
        return await IncludeAll().Where(u => u.Estado).ToListAsync();
    }

    public async Task<Usuario?> GetByIdWithIncludesAsync(int id)
    {
        return await IncludeAll().FirstOrDefaultAsync(u => u.Id == id);
    }
}

public class AtencionRepository : GenericRepository<Atencion>, IAtencionRepository
{
    public AtencionRepository(DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Atencion>> GetAtencionesPorTicketAsync(int ticketId)
    {
        return await _dbSet
            .Where(a => a.TicketId == ticketId)
            .Include(a => a.Usuario)
            .Include(a => a.Area)
            .Include(a => a.AreaDestino)
            .Include(a => a.Servicio)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Atencion>> GetAtencionesPorUsuarioAsync(int usuarioId)
    {
        return await _dbSet.Where(a => a.UsuarioId == usuarioId).ToListAsync();
    }

    public async Task<Atencion?> GetAtencionActivaPorTicketAsync(int ticketId)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.TicketId == ticketId && a.FechaFin == null);
    }
}

public class MarcacionRepository : GenericRepository<Marcacion>, IMarcacionRepository
{
    public MarcacionRepository(DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Marcacion>> GetMarcacionesPorTicketAsync(int ticketId)
    {
        return await _dbSet.Where(m => m.TicketId == ticketId).ToListAsync();
    }

    public async Task<byte> GetUltimoNumeroLlamadoAsync(int ticketId)
    {
        var ultimo = await _dbSet
            .Where(m => m.TicketId == ticketId)
            .OrderByDescending(m => m.NumeroLlamado)
            .FirstOrDefaultAsync();
        return ultimo?.NumeroLlamado ?? 0;
    }
}

public class SesionOperadorRepository : GenericRepository<SesionOperador>, ISesionOperadorRepository
{
    public SesionOperadorRepository(DbContext context) : base(context) { }

    public async Task<SesionOperador?> GetSesionActivaPorUsuarioAsync(int usuarioId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.FechaFin == null);
    }

    public async Task<SesionOperador?> GetSesionActivaPorPuestoAsync(int puestoId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.PuestoId == puestoId && s.FechaFin == null);
    }
}

public class AreaRepository : GenericRepository<Area>, IAreaRepository
{
    public AreaRepository(DbContext context) : base(context) { }

    public async Task<Area?> GetByPrefijoAsync(string prefijo)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.Prefijo == prefijo);
    }
}

public class PuestoRepository : GenericRepository<Puesto>, IPuestoRepository
{
    public PuestoRepository(DbContext context) : base(context) { }

    public async Task<IReadOnlyList<Puesto>> GetPuestosPorAreaAsync(int areaId)
    {
        return await _dbSet
            .Where(p => p.AreaId == areaId && p.Estado)
            .Include(p => p.Sesiones)
            .ToListAsync();
    }
}

