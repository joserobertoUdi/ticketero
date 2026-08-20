using Ticketero.Domain.Entities;

namespace Ticketero.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Atencion>> FinalizarAtencionesVencidasAsync(int minutosMaximo, CancellationToken cancellationToken = default);
    ITicketRepository Tickets { get; }
    IUsuarioRepository Usuarios { get; }
    IAtencionRepository Atenciones { get; }
    IMarcacionRepository Marcaciones { get; }
    ISesionOperadorRepository SesionesOperador { get; }
    IAreaRepository Areas { get; }
    IPuestoRepository Puestos { get; }
    IGenericRepository<Rol> Roles { get; }
    IGenericRepository<Servicio> Servicios { get; }
    IGenericRepository<TipoTicket> TiposTicket { get; }
    IGenericRepository<Prioridad> Prioridades { get; }
    IGenericRepository<EstadoTicket> EstadosTicket { get; }
    IGenericRepository<Kiosko> Kioskos { get; }
    IGenericRepository<Configuracion> Configuraciones { get; }
    IGenericRepository<UsuarioArea> UsuariosArea { get; }
    IGenericRepository<Ubicacion> Ubicaciones { get; }
    IGenericRepository<KioskoArea> KioskoAreas { get; }
    IGenericRepository<ConfiguracionRed> ConfiguracionesRed { get; }
    IGenericRepository<ConfiguracionMultimedia> ConfiguracionesMultimedia { get; }
    IGenericRepository<ConfiguracionImpresora> ConfiguracionesImpresora { get; }
    IGenericRepository<ActivoFijo> ActivosFijos { get; }
    IGenericRepository<ActivoFijoAuditoria> ActivosFijosAuditoria { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


