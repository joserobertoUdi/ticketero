using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketero.Domain.Entities;

namespace Ticketero.Infrastructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("RolId").ValueGeneratedOnAdd();
        builder.Property(r => r.Descripcion).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Logs).HasDefaultValue(0);
        builder.HasIndex(r => r.Descripcion).IsUnique().HasDatabaseName("UQ_Roles_Descripcion");
    }
}

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("UsuarioId").ValueGeneratedOnAdd();
        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Correo).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
        builder.Property(u => u.CodigoSistema).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Logs).HasDefaultValue(0);
        builder.Property(u => u.FechaUltimoAcceso).HasColumnType("datetime2");
        builder.HasIndex(u => u.Correo).IsUnique().HasDatabaseName("UQ_Usuarios_Correo");
        builder.HasIndex(u => u.CodigoSistema).IsUnique().HasDatabaseName("UQ_Usuarios_CodigoSistema");
        builder.HasOne(u => u.Rol).WithMany(r => r.Usuarios).HasForeignKey(u => u.RolId);
    }
}

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("AreasFase");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("AreaId").ValueGeneratedOnAdd();
        builder.Property(a => a.Descripcion).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Prefijo).IsRequired().HasMaxLength(100);
        builder.HasIndex(a => a.Descripcion).IsUnique().HasDatabaseName("UQ_AreasFase_Descripcion");
        builder.HasIndex(a => a.Prefijo).IsUnique().HasDatabaseName("UQ_AreasFase_Prefijo");
    }
}

public class UsuarioAreaConfiguration : IEntityTypeConfiguration<UsuarioArea>
{
    public void Configure(EntityTypeBuilder<UsuarioArea> builder)
    {
        builder.ToTable("UsuariosAreaFase");
        builder.HasKey(ua => ua.Id);
        builder.Property(ua => ua.Id).HasColumnName("UsuarioAreaId").ValueGeneratedOnAdd();
        builder.HasIndex(ua => new { ua.UsuarioId, ua.AreaId }).IsUnique().HasDatabaseName("UQ_UsuariosAreaFase");
        builder.HasOne(ua => ua.Usuario).WithMany(u => u.UsuariosArea).HasForeignKey(ua => ua.UsuarioId);
        builder.HasOne(ua => ua.Area).WithMany(a => a.UsuariosArea).HasForeignKey(ua => ua.AreaId);
    }
}

public class PuestoConfiguration : IEntityTypeConfiguration<Puesto>
{
    public void Configure(EntityTypeBuilder<Puesto> builder)
    {
        builder.ToTable("Puestos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("PuestoId").ValueGeneratedOnAdd();
        builder.Property(p => p.Descripcion).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => new { p.AreaId, p.Descripcion }).IsUnique().HasDatabaseName("UQ_Puestos_Area_Descripcion");
        builder.HasOne(p => p.Area).WithMany(a => a.Puestos).HasForeignKey(p => p.AreaId);
    }
}

public class SesionOperadorConfiguration : IEntityTypeConfiguration<SesionOperador>
{
    public void Configure(EntityTypeBuilder<SesionOperador> builder)
    {
        builder.ToTable("SesionOperador");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("SesionOperadorId").ValueGeneratedOnAdd();
        builder.Property(s => s.FechaInicio).HasColumnType("datetime2").IsRequired();
        builder.Property(s => s.FechaFin).HasColumnType("datetime2");
        builder.HasOne(s => s.Usuario).WithMany(u => u.Sesiones).HasForeignKey(s => s.UsuarioId);
        builder.HasOne(s => s.Puesto).WithMany(p => p.Sesiones).HasForeignKey(s => s.PuestoId);
    }
}

public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    public void Configure(EntityTypeBuilder<Servicio> builder)
    {
        builder.ToTable("Servicios");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("ServicioId").ValueGeneratedOnAdd();
        builder.Property(s => s.Descripcion).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Icono).HasMaxLength(50);
        builder.Property(s => s.AreaId).HasDefaultValue(1);
        builder.HasIndex(s => new { s.AreaId, s.Descripcion }).IsUnique().HasDatabaseName("UQ_Servicios_Area_Descripcion");
        builder.HasOne(s => s.Area).WithMany(a => a.Servicios).HasForeignKey(s => s.AreaId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class TipoTicketConfiguration : IEntityTypeConfiguration<TipoTicket>
{
    public void Configure(EntityTypeBuilder<TipoTicket> builder)
    {
        builder.ToTable("TiposTicket");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("TipoTicketId").ValueGeneratedOnAdd();
        builder.Property(t => t.Descripcion).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Descripcion).IsUnique().HasDatabaseName("UQ_TiposTicket_Descripcion");
    }
}

public class PrioridadConfiguration : IEntityTypeConfiguration<Prioridad>
{
    public void Configure(EntityTypeBuilder<Prioridad> builder)
    {
        builder.ToTable("Prioridades");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("PrioridadId").ValueGeneratedOnAdd();
        builder.Property(p => p.Descripcion).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Nivel).HasColumnType("tinyint").IsRequired();
        builder.Property(p => p.Color).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Descripcion).IsUnique().HasDatabaseName("UQ_Prioridades_Descripcion");
        builder.HasIndex(p => p.Nivel).IsUnique().HasDatabaseName("UQ_Prioridades_Nivel");
    }
}

public class EstadoTicketConfiguration : IEntityTypeConfiguration<EstadoTicket>
{
    public void Configure(EntityTypeBuilder<EstadoTicket> builder)
    {
        builder.ToTable("EstadosTicket");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("EstadoTicketId").ValueGeneratedOnAdd();
        builder.Property(e => e.Descripcion).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Descripcion).IsUnique().HasDatabaseName("UQ_EstadosTicket_Descripcion");
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("TicketId").ValueGeneratedOnAdd();
        builder.Property(t => t.NumeroTicket).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Descripcion).IsRequired().HasMaxLength(500);
        builder.Property(t => t.FechaCreacion).HasColumnType("datetime2").IsRequired();
        builder.Property(t => t.FechaCierre).HasColumnType("datetime2");
        builder.HasIndex(t => t.NumeroTicket).IsUnique().HasDatabaseName("UQ_Tickets_NumeroTicket");
        builder.HasOne(t => t.Servicio).WithMany(s => s.Tickets).HasForeignKey(t => t.ServicioId);
        builder.HasOne(t => t.TipoTicket).WithMany(tt => tt.Tickets).HasForeignKey(t => t.TipoTicketId);
        builder.HasOne(t => t.Prioridad).WithMany(p => p.Tickets).HasForeignKey(t => t.PrioridadId);
        builder.HasOne(t => t.EstadoTicket).WithMany(e => e.Tickets).HasForeignKey(t => t.EstadoTicketId);
        builder.HasOne(t => t.AreaActual).WithMany(a => a.Tickets).HasForeignKey(t => t.AreaActualId);
    }
}

public class AtencionConfiguration : IEntityTypeConfiguration<Atencion>
{
    public void Configure(EntityTypeBuilder<Atencion> builder)
    {
        builder.ToTable("Atencion");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("AtencionId").ValueGeneratedOnAdd();
        builder.Property(a => a.FechaInicio).HasColumnType("datetime2").IsRequired();
        builder.Property(a => a.FechaFin).HasColumnType("datetime2");
        builder.Property(a => a.Observacion).HasMaxLength(500);
        builder.HasIndex(a => new { a.TicketId, a.FechaFin }).HasDatabaseName("IX_Atencion_TicketId_Activa").HasFilter("[FechaFin] IS NULL").IsUnique();
        builder.HasOne(a => a.Ticket).WithMany(t => t.Atenciones).HasForeignKey(a => a.TicketId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.Usuario).WithMany(u => u.Atenciones).HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.Area).WithMany().HasForeignKey(a => a.AreaId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.AreaDestino).WithMany().HasForeignKey(a => a.AreaDestinoId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.Servicio).WithMany(s => s.Atenciones).HasForeignKey(a => a.ServicioId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.EstadoTicket).WithMany(e => e.Atenciones).HasForeignKey(a => a.EstadoTicketId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.SesionOperador).WithMany(s => s.Atenciones).HasForeignKey(a => a.SesionOperadorId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class MarcacionConfiguration : IEntityTypeConfiguration<Marcacion>
{
    public void Configure(EntityTypeBuilder<Marcacion> builder)
    {
        builder.ToTable("Marcacion");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("MarcacionId").ValueGeneratedOnAdd();
        builder.Property(m => m.NumeroLlamado).IsRequired();
        builder.Property(m => m.FechaMarcacion).HasColumnType("datetime2").IsRequired();
        builder.HasOne(m => m.Ticket).WithMany(t => t.Marcaciones).HasForeignKey(m => m.TicketId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(m => m.Usuario).WithMany(u => u.Marcaciones).HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(m => m.Kiosko).WithMany(k => k.Marcaciones).HasForeignKey(m => m.KioskoId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class KioskoConfiguration : IEntityTypeConfiguration<Kiosko>
{
    public void Configure(EntityTypeBuilder<Kiosko> builder)
    {
        builder.ToTable("Kiosko");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id).HasColumnName("KioskoId").ValueGeneratedOnAdd();
        builder.Property(k => k.Descripcion).IsRequired().HasMaxLength(100);
        builder.Property(k => k.Ubicacion).IsRequired().HasMaxLength(150);
        builder.Property(k => k.UbicacionId).HasDefaultValue(1);
        builder.Property(k => k.IpKiosko).HasMaxLength(50);
        builder.Property(k => k.IpEquipo).HasMaxLength(50);
        builder.Property(k => k.MascaraRedEquipo).HasMaxLength(50);
        builder.Property(k => k.GatewayEquipo).HasMaxLength(50);
        builder.Property(k => k.DnsEquipo).HasMaxLength(50);
        builder.HasIndex(k => k.Descripcion).IsUnique().HasDatabaseName("UQ_Kiosko_Descripcion");
        builder.HasOne(k => k.UbicacionNavegacion).WithMany(u => u.Kioskos).HasForeignKey(k => k.UbicacionId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class UbicacionConfiguration : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> builder)
    {
        builder.ToTable("Ubicaciones");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("UbicacionId").ValueGeneratedOnAdd();
        builder.Property(u => u.Descripcion).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Edificio).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Piso).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Sector).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Referencia).HasMaxLength(250);
        builder.HasIndex(u => new { u.Edificio, u.Piso, u.Sector, u.Descripcion }).IsUnique().HasDatabaseName("UQ_Ubicaciones");
    }
}

public class KioskoAreaConfiguration : IEntityTypeConfiguration<KioskoArea>
{
    public void Configure(EntityTypeBuilder<KioskoArea> builder)
    {
        builder.ToTable("KioskoAreas");
        builder.HasKey(ka => ka.Id);
        builder.Property(ka => ka.Id).HasColumnName("KioskoAreaId").ValueGeneratedOnAdd();
        builder.HasIndex(ka => new { ka.KioskoId, ka.AreaId }).IsUnique().HasDatabaseName("UQ_KioskoAreas");
        builder.HasOne(ka => ka.Kiosko).WithMany(k => k.KioskoAreas).HasForeignKey(ka => ka.KioskoId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(ka => ka.Area).WithMany().HasForeignKey(ka => ka.AreaId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class ConfiguracionRedConfiguration : IEntityTypeConfiguration<ConfiguracionRed>
{
    public void Configure(EntityTypeBuilder<ConfiguracionRed> builder)
    {
        builder.ToTable("ConfiguracionRed");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ConfiguracionRedId").ValueGeneratedOnAdd();
        builder.Property(c => c.TipoConexion).IsRequired().HasMaxLength(15);
        builder.Property(c => c.DHCP).HasDefaultValue(true);
        builder.Property(c => c.DireccionIP).HasMaxLength(50);
        builder.Property(c => c.MascaraSubred).HasMaxLength(50);
        builder.Property(c => c.Gateway).HasMaxLength(50);
        builder.Property(c => c.DNSPrimario).HasMaxLength(50);
        builder.Property(c => c.DNSSecundario).HasMaxLength(50);
        builder.HasOne(c => c.Kiosko).WithMany().HasForeignKey(c => c.KioskoId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(c => new { c.KioskoId, c.DireccionIP }).IsUnique().HasDatabaseName("UQ_ConfiguracionRed");
    }
}

public class ConfiguracionMultimediaConfiguration : IEntityTypeConfiguration<ConfiguracionMultimedia>
{
    public void Configure(EntityTypeBuilder<ConfiguracionMultimedia> builder)
    {
        builder.ToTable("ConfiguracionMultimedia");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ConfiguracionMultimediaId").ValueGeneratedOnAdd();
        builder.Property(c => c.NombreContenido).IsRequired().HasMaxLength(200);
        builder.Property(c => c.TipoContenido).IsRequired().HasMaxLength(20);
        builder.Property(c => c.RutaArchivo).IsRequired().HasMaxLength(500);
        builder.Property(c => c.Orden).HasDefaultValue(1);
        builder.Property(c => c.Repetir).HasDefaultValue(true);
        builder.HasOne(c => c.Kiosko).WithMany().HasForeignKey(c => c.KioskoId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(c => new { c.KioskoId, c.NombreContenido }).IsUnique().HasDatabaseName("UQ_ConfiguracionMultimedia");
    }
}

public class ConfiguracionImpresoraConfiguration : IEntityTypeConfiguration<ConfiguracionImpresora>
{
    public void Configure(EntityTypeBuilder<ConfiguracionImpresora> builder)
    {
        builder.ToTable("ConfiguracionImpresora");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ConfiguracionImpresoraId").ValueGeneratedOnAdd();
        builder.Property(c => c.NombreImpresora).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Puerto).HasMaxLength(50);
        builder.Property(c => c.DireccionIP).HasMaxLength(50);
        builder.Property(c => c.TipoConexion).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Copias).HasDefaultValue((byte)1);
        builder.Property(c => c.ImpresionAutomatica).HasDefaultValue(true);
        builder.HasOne(c => c.Kiosko).WithMany().HasForeignKey(c => c.KioskoId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(c => new { c.KioskoId, c.NombreImpresora }).IsUnique().HasDatabaseName("UQ_ConfiguracionImpresora");
    }
}

public class ActivoFijoConfiguration : IEntityTypeConfiguration<ActivoFijo>
{
    public void Configure(EntityTypeBuilder<ActivoFijo> builder)
    {
        builder.ToTable("ActivosFijos");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("ActivoFijoId").ValueGeneratedOnAdd();
        builder.Property(a => a.TipoActivo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.NumeroActivo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Descripcion).HasMaxLength(200);
        builder.Property(a => a.Marca).HasMaxLength(100);
        builder.Property(a => a.Modelo).HasMaxLength(100);
        builder.Property(a => a.Serie).HasMaxLength(100);
        builder.Property(a => a.CreadoPor).HasMaxLength(100);
        builder.Property(a => a.UltimaModificacionPor).HasMaxLength(100);
        builder.HasOne(a => a.Kiosko).WithMany(k => k.ActivosFijos).HasForeignKey(a => a.KioskoId).OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(a => a.Auditorias).WithOne(aa => aa.ActivoFijo).HasForeignKey(aa => aa.ActivoFijoId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(a => new { a.KioskoId, a.NumeroActivo }).IsUnique().HasDatabaseName("UQ_ActivosFijos");
    }
}

public class ActivoFijoAuditoriaConfiguration : IEntityTypeConfiguration<ActivoFijoAuditoria>
{
    public void Configure(EntityTypeBuilder<ActivoFijoAuditoria> builder)
    {
        builder.ToTable("ActivosFijosAuditoria");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("AuditoriaId").ValueGeneratedOnAdd();
        builder.Property(a => a.Accion).IsRequired().HasMaxLength(20);
        builder.Property(a => a.UsuarioNombre).IsRequired().HasMaxLength(100);
        builder.Property(a => a.CambioResumen).HasMaxLength(500);
        builder.Property(a => a.FechaCambio).IsRequired().HasDefaultValueSql("SYSDATETIME()");
        builder.HasOne(a => a.Kiosko).WithMany().HasForeignKey(a => a.KioskoId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(a => a.FechaCambio).HasDatabaseName("IX_ActivosFijosAuditoria_Fecha");
    }
}

public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> builder)
    {
        builder.ToTable("Configuraciones");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ConfiguracionId").ValueGeneratedOnAdd();
        builder.Property(c => c.Clave).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Valor).IsRequired().HasMaxLength(500);
        builder.Property(c => c.Descripcion).HasMaxLength(300);
        builder.HasIndex(c => c.Clave).IsUnique().HasDatabaseName("UQ_Configuraciones_Clave");
        builder.HasIndex(c => c.Estado).HasDatabaseName("IX_Configuraciones_Estado");
    }
}

