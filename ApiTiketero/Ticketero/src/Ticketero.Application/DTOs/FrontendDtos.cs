using System.ComponentModel.DataAnnotations;

namespace Ticketero.Application.DTOs;

public class UserResponse
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? AreaId { get; set; }
    public string? AreaNombre { get; set; }
    public string? Puesto { get; set; }
    public List<int> AreasAtencion { get; set; } = new();
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

public class CreateUserRequest
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? AreaId { get; set; }
    public List<int> AreasAtencion { get; set; } = new();
}

public class UpdateUserRequest
{
    public string? NombreCompleto { get; set; }
    public string? Correo { get; set; }
    public string? Password { get; set; }
    public string? Rol { get; set; }
    public int? AreaId { get; set; }
    public List<int>? AreasAtencion { get; set; }
    public bool? Activo { get; set; }
}

public class TicketResponse
{
    public int Id { get; set; }
    public int? AtencionId { get; set; }
    public string CodigoTicket { get; set; } = string.Empty;
    public string TipoTicket { get; set; } = string.Empty;
    public int AreaId { get; set; }
    public string AreaNombre { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? LlamadoPorUserId { get; set; }
    public string? LlamadoPorUserName { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? TiempoEsperaSegundos { get; set; }
    public int? TiempoAtencionSegundos { get; set; }
    public string? TiempoAtencionFormateado { get; set; }
    public int Prioridad { get; set; }
    public string? DerivadoDe { get; set; }
    public string? DerivadoDeNombre { get; set; }
    public string? Observacion { get; set; }
}

public class CreateTicketFrontendRequest
{
    public int AreaId { get; set; }
    public int? ServicioId { get; set; }
    public int TipoTicketId { get; set; } = 1;
    public int PrioridadId { get; set; } = 3;
    public string? Descripcion { get; set; }
}

public class CallTicketRequest
{
    public int UserId { get; set; }
    public int KioskoId { get; set; }
}

public class StartAttentionRequest
{
    public int UserId { get; set; }
    public int PuestoId { get; set; }
}

public class CompleteTicketRequest
{
    public int UserId { get; set; }
    public string? Observacion { get; set; }
}

public class CancelTicketRequest
{
    public string? Motivo { get; set; }
}

public class PendingTicketsResponse
{
    public List<TicketResponse> Tickets { get; set; } = new();
}

public class AreaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Prefijo { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class AreaCreateRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Prefijo { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}

public class AreaUpdateRequest
{
    public string? Nombre { get; set; }
    public string? Prefijo { get; set; }
    public string? LogoUrl { get; set; }
    public bool? Activo { get; set; }
}

public class MultimediaUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
}

public class KioskoMultimediaConfigResponse
{
    public int KioskoId { get; set; }
    public List<ConfiguracionMultimediaItem> Videos { get; set; } = new();
    public List<ConfiguracionMultimediaItem> Logos { get; set; } = new();
}

public class ConfiguracionMultimediaItem
{
    public int Id { get; set; }
    public string NombreContenido { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Repetir { get; set; }
    public bool Activo { get; set; }
}

public class DashboardSummaryResponse
{
    public int TotalTickets { get; set; }
    public int TotalAtendidos { get; set; }
    public int TotalPendientes { get; set; }
    public int TiempoPromedioAtencion { get; set; }
    public string TiempoPromedioAtencionFormateado { get; set; } = "00:00";
    public int TiempoPromedioEspera { get; set; }
    public string TiempoPromedioEsperaFormateado { get; set; } = "00:00";
    public PeriodoInfo? Periodo { get; set; }
}

public class PeriodoInfo
{
    public string Inicio { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
}

public class AutoRegistrarRequest
{
    public string? IpEquipo { get; set; }
    public string? MascaraRedEquipo { get; set; }
    public string? GatewayEquipo { get; set; }
    public string? DnsEquipo { get; set; }
}

public class KioskoFisicoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public int UbicacionId { get; set; }
    public string? UbicacionNombre { get; set; }
    public bool Activo { get; set; }
    public bool EsHuerfano { get; set; }
    public string? IpKiosko { get; set; }
    public List<int> AreaIds { get; set; } = new();

    public string? NombreImpresora { get; set; }
    public string? PuertoImpresora { get; set; }
    public string? IpImpresora { get; set; }
    public string? TipoConexionImpresora { get; set; }
    public int AnchoPapelMM { get; set; }
    public byte Copias { get; set; } = 1;
    public bool ImpresionAutomatica { get; set; } = true;

    public string? IpEquipo { get; set; }
    public string? MascaraRedEquipo { get; set; }
    public string? GatewayEquipo { get; set; }
    public string? DnsEquipo { get; set; }

    public string? TipoConexionRed { get; set; }
    public bool DHCP { get; set; } = true;
    public string? DireccionIP { get; set; }
    public string? MascaraSubred { get; set; }
    public string? GatewayRed { get; set; }
    public string? DnsPrimario { get; set; }
    public string? DnsSecundario { get; set; }
    public int? PuertoRed { get; set; }
    public List<ActivoFijoResponse> ActivosFijos { get; set; } = new();
    public string? LogoUrl { get; set; }
    public string? VideoUrl { get; set; }
}

public class ActivoFijoResponse
{
    public int Id { get; set; }
    public string TipoActivo { get; set; } = string.Empty;
    public string NumeroActivo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }
    public string? CreadoPor { get; set; }
    public string? UltimaModificacionPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

public class DetectarIpRequest
{
    public int KioskoId { get; set; }
    public string DireccionIP { get; set; } = string.Empty;
}

public class KioskoFisicoCreateRequest
{
    [Required(ErrorMessage = "El nombre del kiosko es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es obligatoria")]
    [StringLength(150, ErrorMessage = "La ubicación no puede exceder 150 caracteres")]
    public string Ubicacion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La ubicaciónId debe ser un valor positivo")]
    public int UbicacionId { get; set; } = 1;

    [StringLength(50)]
    public string? IpKiosko { get; set; }

    [Required(ErrorMessage = "Debe seleccionar al menos un área")]
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos un área")]
    public List<int> AreaIds { get; set; } = new();

    [StringLength(150)]
    public string? NombreImpresora { get; set; }
    [StringLength(50)]
    public string? PuertoImpresora { get; set; }
    [StringLength(50)]
    public string? IpImpresora { get; set; }
    [StringLength(20)]
    public string? TipoConexionImpresora { get; set; }
    [Range(58, 80, ErrorMessage = "El ancho de papel debe ser 58mm u 80mm")]
    public int AnchoPapelMM { get; set; } = 80;
    [Range(1, 5, ErrorMessage = "Las copias deben estar entre 1 y 5")]
    public byte Copias { get; set; } = 1;
    public bool ImpresionAutomatica { get; set; } = true;

    [StringLength(50)]
    public string? IpEquipo { get; set; }
    [StringLength(50)]
    public string? MascaraRedEquipo { get; set; }
    [StringLength(50)]
    public string? GatewayEquipo { get; set; }
    [StringLength(50)]
    public string? DnsEquipo { get; set; }

    [StringLength(15)]
    public string? TipoConexionRed { get; set; }
    public bool DHCP { get; set; } = true;
    [StringLength(50)]
    public string? DireccionIP { get; set; }
    [StringLength(50)]
    public string? MascaraSubred { get; set; }
    [StringLength(50)]
    public string? GatewayRed { get; set; }
    [StringLength(50)]
    public string? DnsPrimario { get; set; }
    [StringLength(50)]
    public string? DnsSecundario { get; set; }
    [Range(1, 65535, ErrorMessage = "El puerto debe estar entre 1 y 65535")]
    public int? PuertoRed { get; set; }
    public List<ActivoFijoRequest>? ActivosFijos { get; set; }
    [StringLength(500)]
    public string? LogoUrl { get; set; }
    [StringLength(500)]
    public string? VideoUrl { get; set; }
}

public class ActivoFijoRequest
{
    [Required(ErrorMessage = "El tipo de activo es obligatorio")]
    [StringLength(50)]
    public string TipoActivo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de activo es obligatorio")]
    [StringLength(50)]
    public string NumeroActivo { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Descripcion { get; set; }
    [StringLength(100)]
    public string? Marca { get; set; }
    [StringLength(100)]
    public string? Modelo { get; set; }
    [StringLength(100)]
    public string? Serie { get; set; }
}

public class PrinterConfigResponse
{
    public int KioskoId { get; set; }
    public string? TipoConexion { get; set; }
    public string? DireccionIP { get; set; }
    public string? Puerto { get; set; }
    public string? NombreImpresora { get; set; }
    public int AnchoPapelMM { get; set; } = 80;
    public byte Copias { get; set; } = 1;
    public bool ImpresionAutomatica { get; set; } = true;
}

public class PrinterConfigUpdateRequest
{
    public string? TipoConexion { get; set; }
    public string? DireccionIP { get; set; }
    public string? Puerto { get; set; }
    public string? NombreImpresora { get; set; }
    public int AnchoPapelMM { get; set; } = 80;
    public byte Copias { get; set; } = 1;
    public bool ImpresionAutomatica { get; set; } = true;
}

public class KioskoFisicoUpdateRequest
{
    [StringLength(100)]
    public string? Nombre { get; set; }
    [StringLength(150)]
    public string? Ubicacion { get; set; }
    [Range(1, int.MaxValue)]
    public int? UbicacionId { get; set; }
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos un área")]
    public List<int>? AreaIds { get; set; }
    public bool? Activo { get; set; }
    [StringLength(50)]
    public string? IpKiosko { get; set; }
    public List<ActivoFijoRequest>? ActivosFijos { get; set; }

    [StringLength(150)]
    public string? NombreImpresora { get; set; }
    [StringLength(50)]
    public string? PuertoImpresora { get; set; }
    [StringLength(50)]
    public string? IpImpresora { get; set; }
    [StringLength(20)]
    public string? TipoConexionImpresora { get; set; }
    [Range(58, 80)]
    public int? AnchoPapelMM { get; set; }
    [Range(1, 5)]
    public byte? Copias { get; set; }
    public bool? ImpresionAutomatica { get; set; }

    [StringLength(50)]
    public string? IpEquipo { get; set; }
    [StringLength(50)]
    public string? MascaraRedEquipo { get; set; }
    [StringLength(50)]
    public string? GatewayEquipo { get; set; }
    [StringLength(50)]
    public string? DnsEquipo { get; set; }

    [StringLength(15)]
    public string? TipoConexionRed { get; set; }
    public bool? DHCP { get; set; }
    [StringLength(50)]
    public string? DireccionIP { get; set; }
    [StringLength(50)]
    public string? MascaraSubred { get; set; }
    [StringLength(50)]
    public string? GatewayRed { get; set; }
    [StringLength(50)]
    public string? DnsPrimario { get; set; }
    [StringLength(50)]
    public string? DnsSecundario { get; set; }
    [Range(1, 65535)]
    public int? PuertoRed { get; set; }
    public string? LogoUrl { get; set; }
    public string? VideoUrl { get; set; }
}
