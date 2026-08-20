namespace Ticketero.Application.DTOs;

public class TicketDto
{
    public int TicketId { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;
    public string TipoTicket { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string EstadoActual { get; set; } = string.Empty;
    public string AreaActual { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCierre { get; set; }
}

public class CreateTicketRequest
{
    public int ServicioId { get; set; }
    public int TipoTicketId { get; set; }
    public int PrioridadId { get; set; }
    public int AreaActualId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class CreateTicketResponse
{
    public int TicketId { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public class AtenderTicketRequest
{
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }
    public int AreaId { get; set; }
    public int ServicioId { get; set; }
    public int SesionOperadorId { get; set; }
}

public class AtenderTicketResponse
{
    public int AtencionId { get; set; }
    public int? TicketId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public class CerrarTicketRequest
{
    public int TicketId { get; set; }
    public int AtencionId { get; set; }
    public string? Observacion { get; set; }
}

public class DerivarTicketRequest
{
    public int TicketId { get; set; }
    public int AtencionId { get; set; }
    public int AreaDestinoId { get; set; }
    public string? Observacion { get; set; }
}
