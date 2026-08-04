using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Ticket : EntityBase
{
    [StringLength(30)]
    public string NumeroTicket { get; set; } = string.Empty;

    public int ServicioId { get; set; }
    public int TipoTicketId { get; set; }
    public int PrioridadId { get; set; }
    public int EstadoTicketId { get; set; }
    public int AreaActualId { get; set; }

    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public Servicio? Servicio { get; set; }
    public TipoTicket? TipoTicket { get; set; }
    public Prioridad? Prioridad { get; set; }
    public EstadoTicket? EstadoTicket { get; set; }
    public Area? AreaActual { get; set; }
    public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
    public ICollection<Marcacion> Marcaciones { get; set; } = new List<Marcacion>();
}
