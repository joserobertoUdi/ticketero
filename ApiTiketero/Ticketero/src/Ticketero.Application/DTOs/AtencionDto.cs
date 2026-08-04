namespace Ticketero.Application.DTOs;

public class AtencionDto
{
    public int AtencionId { get; set; }
    public int TicketId { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;
    public string EstadoTicket { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? TiempoAtencionSegundos { get; set; }
    public bool FueDerivado { get; set; }
    public string? AreaDestino { get; set; }
    public string? Observacion { get; set; }
}

public class AttentionHistoryItem
{
    public int AtencionId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string AreaNombre { get; set; } = string.Empty;
    public string ServicioNombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? TiempoAtencionSegundos { get; set; }
    public string? TiempoAtencionFormateado { get; set; }
    public bool FueDerivado { get; set; }
    public string? AreaDestinoNombre { get; set; }
    public string? Observacion { get; set; }
}
