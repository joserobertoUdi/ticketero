using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Atencion : EntityBase
{
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }
    public int AreaId { get; set; }
    public int ServicioId { get; set; }
    public int EstadoTicketId { get; set; }
    public int? SesionOperadorId { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? TiempoAtencionSegundos { get; set; }

    public bool FueDerivado { get; set; }
    public int? AreaDestinoId { get; set; }

    [StringLength(500)]
    public string? Observacion { get; set; }

    public Ticket? Ticket { get; set; }
    public Usuario? Usuario { get; set; }
    public Area? Area { get; set; }
    public Area? AreaDestino { get; set; }
    public Servicio? Servicio { get; set; }
    public EstadoTicket? EstadoTicket { get; set; }
    public SesionOperador? SesionOperador { get; set; }
}
