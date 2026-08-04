using Microsoft.EntityFrameworkCore;
using Ticketero.Domain.Entities;

namespace Ticketero.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(TicketeroDbContext context, string? defaultPassword = null)
    {
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Rol { Descripcion = "Administrador" },
                new Rol { Descripcion = "Supervisor" },
                new Rol { Descripcion = "Operador" },
                new Rol { Descripcion = "Llamador" }
            );
            await context.SaveChangesAsync();
        }

        var estadosRequeridos = new[] { "Nuevo", "Asignado", "En Proceso", "En Espera", "Resuelto", "Cerrado", "Cancelado", "Llamado" };
        var estadosExistentes = await context.EstadosTicket.Select(e => e.Descripcion).ToListAsync();
        var estadosFaltantes = estadosRequeridos.Where(e => !estadosExistentes.Contains(e)).ToList();

        if (estadosFaltantes.Any())
        {
            foreach (var est in estadosFaltantes)
            {
                context.EstadosTicket.Add(new EstadoTicket { Descripcion = est });
            }
            await context.SaveChangesAsync();
        }

        if (!await context.Prioridades.AnyAsync())
        {
            context.Prioridades.AddRange(
                new Prioridad { Descripcion = "Critica", Nivel = 1, Color = "Rojo" },
                new Prioridad { Descripcion = "Alta", Nivel = 2, Color = "Naranja" },
                new Prioridad { Descripcion = "Media", Nivel = 3, Color = "Amarillo" },
                new Prioridad { Descripcion = "Baja", Nivel = 4, Color = "Verde" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.TiposTicket.AnyAsync())
        {
            context.TiposTicket.AddRange(
                new TipoTicket { Descripcion = "Incidente" },
                new TipoTicket { Descripcion = "Solicitud" },
                new TipoTicket { Descripcion = "Problema" },
                new TipoTicket { Descripcion = "Cambio" },
                new TipoTicket { Descripcion = "Consulta" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Areas.AnyAsync())
        {
            context.Areas.AddRange(
                new Area { Descripcion = "Caja", Prefijo = "CAJ" },
                new Area { Descripcion = "Informes", Prefijo = "INF" },
                new Area { Descripcion = "Atención al Cliente", Prefijo = "ATE" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Servicios.AnyAsync())
        {
            var areaCaja = await context.Areas.FirstAsync(a => a.Descripcion == "Caja");
            context.Servicios.AddRange(
                new Servicio { Descripcion = "PAGO", AreaId = areaCaja.Id },
                new Servicio { Descripcion = "INFORMACION", AreaId = areaCaja.Id },
                new Servicio { Descripcion = "CONSULTA", AreaId = areaCaja.Id },
                new Servicio { Descripcion = "RETIRO", AreaId = areaCaja.Id }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Ubicaciones.AnyAsync())
        {
            context.Ubicaciones.AddRange(
                new Ubicacion { Descripcion = "Caja General", Edificio = "Principal", Piso = "Planta Baja", Sector = "Caja" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Kioskos.AnyAsync())
        {
            var ubicacionDefault = await context.Ubicaciones.FirstAsync();
            context.Kioskos.AddRange(
                new Kiosko { Descripcion = "Kiosko Principal", Ubicacion = "Entrada", UbicacionId = ubicacionDefault.Id },
                new Kiosko { Descripcion = "Kiosko Secundario", Ubicacion = "Pasillo", UbicacionId = ubicacionDefault.Id }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Puestos.AnyAsync())
        {
            var areaCaja = await context.Areas.FirstAsync(a => a.Descripcion == "Caja");
            context.Puestos.AddRange(
                new Puesto { AreaId = areaCaja.Id, Descripcion = "Caja 1" },
                new Puesto { AreaId = areaCaja.Id, Descripcion = "Caja 2" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Usuarios.AnyAsync() && !string.IsNullOrEmpty(defaultPassword))
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
            context.Usuarios.AddRange(
                new Usuario { NombreUsuario = "admin", Nombre = "Administrador", Apellido = "Sistema", Correo = "admin@empresa.com", PasswordHash = hash, CodigoSistema = "ADM001", RolId = 1 },
                new Usuario { NombreUsuario = "jperez", Nombre = "Juan", Apellido = "Perez", Correo = "jperez@empresa.com", PasswordHash = hash, CodigoSistema = "SPV001", RolId = 2 },
                new Usuario { NombreUsuario = "mgarcia", Nombre = "Maria", Apellido = "Garcia", Correo = "mgarcia@empresa.com", PasswordHash = hash, CodigoSistema = "OPE001", RolId = 3 },
                new Usuario { NombreUsuario = "rtorres", Nombre = "Roberto", Apellido = "Torres", Correo = "rtorres@empresa.com", PasswordHash = hash, CodigoSistema = "LLA001", RolId = 4 }
            );
            await context.SaveChangesAsync();
        }
    }
}