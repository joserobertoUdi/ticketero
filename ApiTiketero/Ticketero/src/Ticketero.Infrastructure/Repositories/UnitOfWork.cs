using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Infrastructure.Data;

namespace Ticketero.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TicketeroDbContext _context;
    private ITicketRepository? _tickets;
    private IUsuarioRepository? _usuarios;
    private IAtencionRepository? _atenciones;
    private IMarcacionRepository? _marcaciones;
    private ISesionOperadorRepository? _sesionesOperador;
    private IAreaRepository? _areas;
    private IPuestoRepository? _puestos;
    private IGenericRepository<Rol>? _roles;
    private IGenericRepository<Servicio>? _servicios;
    private IGenericRepository<TipoTicket>? _tiposTicket;
    private IGenericRepository<Prioridad>? _prioridades;
    private IGenericRepository<EstadoTicket>? _estadosTicket;
    private IGenericRepository<Kiosko>? _kioskos;
    private IGenericRepository<Configuracion>? _configuraciones;
    private IGenericRepository<UsuarioArea>? _usuariosArea;
    private IGenericRepository<Ubicacion>? _ubicaciones;
    private IGenericRepository<KioskoArea>? _kioskoAreas;
    private IGenericRepository<ConfiguracionRed>? _configuracionesRed;
    private IGenericRepository<ConfiguracionMultimedia>? _configuracionesMultimedia;
    private IGenericRepository<ConfiguracionImpresora>? _configuracionesImpresora;
    private IGenericRepository<ActivoFijo>? _activosFijos;
    private IGenericRepository<ActivoFijoAuditoria>? _activosFijosAuditoria;

    public UnitOfWork(TicketeroDbContext context)
    {
        _context = context;
    }

    public ITicketRepository Tickets =>
        _tickets ??= new TicketRepository(_context);

    public IUsuarioRepository Usuarios =>
        _usuarios ??= new UsuarioRepository(_context);

    public IAtencionRepository Atenciones =>
        _atenciones ??= new AtencionRepository(_context);

    public IMarcacionRepository Marcaciones =>
        _marcaciones ??= new MarcacionRepository(_context);

    public ISesionOperadorRepository SesionesOperador =>
        _sesionesOperador ??= new SesionOperadorRepository(_context);

    public IAreaRepository Areas =>
        _areas ??= new AreaRepository(_context);

    public IPuestoRepository Puestos =>
        _puestos ??= new PuestoRepository(_context);

    public IGenericRepository<Rol> Roles =>
        _roles ??= new GenericRepository<Rol>(_context);

    public IGenericRepository<Servicio> Servicios =>
        _servicios ??= new GenericRepository<Servicio>(_context);

    public IGenericRepository<TipoTicket> TiposTicket =>
        _tiposTicket ??= new GenericRepository<TipoTicket>(_context);

    public IGenericRepository<Prioridad> Prioridades =>
        _prioridades ??= new GenericRepository<Prioridad>(_context);

    public IGenericRepository<EstadoTicket> EstadosTicket =>
        _estadosTicket ??= new GenericRepository<EstadoTicket>(_context);

    public IGenericRepository<Kiosko> Kioskos =>
        _kioskos ??= new GenericRepository<Kiosko>(_context);

    public IGenericRepository<Configuracion> Configuraciones =>
        _configuraciones ??= new GenericRepository<Configuracion>(_context);

    public IGenericRepository<UsuarioArea> UsuariosArea =>
        _usuariosArea ??= new GenericRepository<UsuarioArea>(_context);

    public IGenericRepository<Ubicacion> Ubicaciones =>
        _ubicaciones ??= new GenericRepository<Ubicacion>(_context);

    public IGenericRepository<KioskoArea> KioskoAreas =>
        _kioskoAreas ??= new GenericRepository<KioskoArea>(_context);

    public IGenericRepository<ConfiguracionRed> ConfiguracionesRed =>
        _configuracionesRed ??= new GenericRepository<ConfiguracionRed>(_context);

    public IGenericRepository<ConfiguracionMultimedia> ConfiguracionesMultimedia =>
        _configuracionesMultimedia ??= new GenericRepository<ConfiguracionMultimedia>(_context);

    public IGenericRepository<ConfiguracionImpresora> ConfiguracionesImpresora =>
        _configuracionesImpresora ??= new GenericRepository<ConfiguracionImpresora>(_context);

    public IGenericRepository<ActivoFijo> ActivosFijos =>
        _activosFijos ??= new GenericRepository<ActivoFijo>(_context);

    public IGenericRepository<ActivoFijoAuditoria> ActivosFijosAuditoria =>
        _activosFijosAuditoria ??= new GenericRepository<ActivoFijoAuditoria>(_context);

    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _currentTransaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            await _currentTransaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            await _currentTransaction.RollbackAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
