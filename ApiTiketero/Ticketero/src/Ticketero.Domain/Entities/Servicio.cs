using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Servicio : EntityBase
{
    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Icono { get; set; }

    public int AreaId { get; set; }

    public Area? Area { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
}
