using Ticketero.Application.DTOs;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Mapping;

public static class MappingService
{
    private static readonly Dictionary<string, string> RoleToFrontend = new()
    {
        ["Administrador"] = "administrador",
        ["Supervisor"] = "supervisor",
        ["Operador"] = "operador",
        ["Llamador"] = "llamador"
    };

    private static readonly Dictionary<int, string> EstadoToStatus = new()
    {
        [1] = "pendiente",
        [2] = "pendiente",
        [3] = "en_atencion",
        [4] = "en_atencion",
        [5] = "completado",
        [6] = "completado",
        [7] = "cancelado",
        [8] = "llamado"
    };

    public static string MapRolToFrontend(string rolBd)
    {
        return RoleToFrontend.TryGetValue(rolBd, out var frontend) ? frontend : rolBd.ToLower();
    }

    public static string? MapRolToBd(string? rolFrontend)
    {
        if (rolFrontend == null) return null;
        foreach (var kvp in RoleToFrontend)
        {
            if (kvp.Value == rolFrontend) return kvp.Key;
        }
        return null;
    }

    public static string MapEstadoToStatus(int estadoTicketId)
    {
        return EstadoToStatus.TryGetValue(estadoTicketId, out var status) ? status : "pendiente";
    }

    public static string FormatearTiempo(int segundos)
    {
        var ts = TimeSpan.FromSeconds(segundos);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public static UserResponse MapToUserResponse(Usuario usuario, Rol? rol, List<UsuarioArea> areas)
    {
        var areaPrincipal = areas.FirstOrDefault();
        return new UserResponse
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
            Email = usuario.Correo,
            Rol = MapRolToFrontend(rol?.Descripcion ?? ""),
            AreaId = areaPrincipal?.AreaId,
            AreaNombre = areaPrincipal?.Area?.Descripcion,
            AreasAtencion = areas.Select(a => a.AreaId).ToList(),
            Activo = usuario.Estado,
            UltimoAcceso = usuario.FechaUltimoAcceso
        };
    }

    public static TicketResponse MapToTicketResponse(Ticket ticket)
    {
        var ultimaAtencion = ticket.Atenciones?.OrderByDescending(a => a.Id).FirstOrDefault();
        var primeraAtencion = ticket.Atenciones?.OrderBy(a => a.Id).FirstOrDefault();
        var ultimaMarcacion = ticket.Marcaciones?.OrderByDescending(m => m.Id).FirstOrDefault();

        _ = ticket.AreaActual?.Descripcion ?? "";

        int? tiempoEspera = null;
        if (primeraAtencion?.FechaInicio != null)
        {
            tiempoEspera = (int)(primeraAtencion.FechaInicio - ticket.FechaCreacion).TotalSeconds;
            if (tiempoEspera < 0) tiempoEspera = 0;
        }

        return new TicketResponse
        {
            Id = ticket.Id,
            CodigoTicket = ticket.NumeroTicket,
            TipoTicket = ticket.TipoTicket?.Descripcion ?? "",
            AreaId = ticket.AreaActualId,
            AreaNombre = ticket.AreaActual?.Descripcion ?? "",
            Status = MapEstadoToStatus(ticket.EstadoTicketId),
            LlamadoPorUserId = ultimaMarcacion?.UsuarioId,
            LlamadoPorUserName = ultimaMarcacion?.Usuario != null
                ? $"{ultimaMarcacion.Usuario.Nombre} {ultimaMarcacion.Usuario.Apellido}"
                : null,
            CreadoEn = ticket.FechaCreacion,
            UpdatedAt = ticket.FechaCierre ?? ultimaAtencion?.FechaFin,
            TiempoEsperaSegundos = tiempoEspera,
            TiempoAtencionSegundos = ultimaAtencion?.TiempoAtencionSegundos,
            TiempoAtencionFormateado = ultimaAtencion?.TiempoAtencionSegundos != null
                ? FormatearTiempo(ultimaAtencion.TiempoAtencionSegundos.Value)
                : null,
            Prioridad = ticket.Prioridad?.Nivel ?? 0,
            DerivadoDe = ultimaAtencion?.FueDerivado == true ? ultimaAtencion?.AreaId.ToString() : null,
            DerivadoDeNombre = ultimaAtencion?.FueDerivado == true ? ultimaAtencion?.Area?.Descripcion : null,
            Observacion = ultimaAtencion?.Observacion
        };
    }

    public static AreaResponse MapToAreaResponse(Area area)
    {
        return new AreaResponse
        {
            Id = area.Id,
            Nombre = area.Descripcion,
            Prefijo = area.Prefijo,
            LogoUrl = area.LogoUrl ?? "",
            Activo = area.Estado
        };
    }

    public static KioskoFisicoResponse MapToKioskoFisicoResponse(
        Kiosko kiosko,
        Ubicacion? ubicacion,
        ConfiguracionImpresora? impresora,
        ConfiguracionRed? red,
        List<int>? areaIds,
        List<ActivoFijo>? activosFijos = null,
        string? logoUrl = null,
        string? videoUrl = null)
    {
        return new KioskoFisicoResponse
        {
            Id = kiosko.Id,
            Nombre = kiosko.Descripcion,
            Ubicacion = kiosko.Ubicacion,
            UbicacionId = kiosko.UbicacionId,
            UbicacionNombre = ubicacion?.Descripcion,
            Activo = kiosko.Estado,
            EsHuerfano = impresora == null && (areaIds == null || areaIds.Count == 0) && kiosko.Ubicacion == "Sin asignar",
            IpKiosko = kiosko.IpKiosko,
            AreaIds = areaIds ?? new(),

            NombreImpresora = impresora?.NombreImpresora,
            PuertoImpresora = impresora?.Puerto,
            IpImpresora = impresora?.DireccionIP,
            TipoConexionImpresora = impresora?.TipoConexion,
            AnchoPapelMM = impresora?.AnchoPapelMM ?? 80,
            Copias = impresora?.Copias ?? (byte)1,
            ImpresionAutomatica = impresora?.ImpresionAutomatica ?? true,

            IpEquipo = kiosko.IpEquipo,
            MascaraRedEquipo = kiosko.MascaraRedEquipo,
            GatewayEquipo = kiosko.GatewayEquipo,
            DnsEquipo = kiosko.DnsEquipo,

            TipoConexionRed = red?.TipoConexion,
            DHCP = red?.DHCP ?? true,
            DireccionIP = red?.DireccionIP,
            MascaraSubred = red?.MascaraSubred,
            GatewayRed = red?.Gateway,
            DnsPrimario = red?.DNSPrimario,
            DnsSecundario = red?.DNSSecundario,
            PuertoRed = red?.Puerto,
            ActivosFijos = activosFijos?.Select(a => new ActivoFijoResponse
            {
                Id = a.Id,
                TipoActivo = a.TipoActivo,
                NumeroActivo = a.NumeroActivo,
                Descripcion = a.Descripcion,
                Marca = a.Marca,
                Modelo = a.Modelo,
                Serie = a.Serie,
                CreadoPor = a.CreadoPor,
                UltimaModificacionPor = a.UltimaModificacionPor,
                FechaModificacion = a.FechaModificacion
            }).ToList() ?? new(),
            LogoUrl = logoUrl,
            VideoUrl = videoUrl
        };
    }
}
