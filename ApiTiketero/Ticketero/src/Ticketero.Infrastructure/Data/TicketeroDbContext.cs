using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ticketero.Domain.Entities;
using Ticketero.Infrastructure.Data.Configurations;

namespace Ticketero.Infrastructure.Data;

public class TicketeroDbContext : DbContext
{
    public TicketeroDbContext(DbContextOptions<TicketeroDbContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<UsuarioArea> UsuariosArea => Set<UsuarioArea>();
    public DbSet<Puesto> Puestos => Set<Puesto>();
    public DbSet<SesionOperador> SesionesOperador => Set<SesionOperador>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<TipoTicket> TiposTicket => Set<TipoTicket>();
    public DbSet<Prioridad> Prioridades => Set<Prioridad>();
    public DbSet<EstadoTicket> EstadosTicket => Set<EstadoTicket>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Atencion> Atenciones => Set<Atencion>();
    public DbSet<Marcacion> Marcaciones => Set<Marcacion>();
    public DbSet<Kiosko> Kioskos => Set<Kiosko>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<KioskoArea> KioskoAreas => Set<KioskoArea>();
    public DbSet<ConfiguracionRed> ConfiguracionesRed => Set<ConfiguracionRed>();
    public DbSet<ConfiguracionMultimedia> ConfiguracionesMultimedia => Set<ConfiguracionMultimedia>();
    public DbSet<ConfiguracionImpresora> ConfiguracionesImpresora => Set<ConfiguracionImpresora>();
    public DbSet<ActivoFijo> ActivosFijos => Set<ActivoFijo>();
    public DbSet<ActivoFijoAuditoria> ActivosFijosAuditoria => Set<ActivoFijoAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RolConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new AreaConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioAreaConfiguration());
        modelBuilder.ApplyConfiguration(new PuestoConfiguration());
        modelBuilder.ApplyConfiguration(new SesionOperadorConfiguration());
        modelBuilder.ApplyConfiguration(new ServicioConfiguration());
        modelBuilder.ApplyConfiguration(new TipoTicketConfiguration());
        modelBuilder.ApplyConfiguration(new PrioridadConfiguration());
        modelBuilder.ApplyConfiguration(new EstadoTicketConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new AtencionConfiguration());
        modelBuilder.ApplyConfiguration(new MarcacionConfiguration());
        modelBuilder.ApplyConfiguration(new KioskoConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionConfiguration());
        modelBuilder.ApplyConfiguration(new UbicacionConfiguration());
        modelBuilder.ApplyConfiguration(new KioskoAreaConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionRedConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionMultimediaConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionImpresoraConfiguration());
        modelBuilder.ApplyConfiguration(new ActivoFijoConfiguration());
        modelBuilder.ApplyConfiguration(new ActivoFijoAuditoriaConfiguration());

        modelBuilder.HasDefaultSchema("dbo");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(EntityBase.Estado));
                var condition = Expression.Equal(property, Expression.Constant(true));
                var lambda = Expression.Lambda(condition, parameter);
                entityType.SetQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaReg = DateTime.UtcNow;
                if (entry.Entity.Ride == Guid.Empty)
                    typeof(EntityBase).GetProperty(nameof(EntityBase.Ride))?.SetValue(entry.Entity, Guid.NewGuid());
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
