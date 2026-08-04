using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Prioridad : EntityBase
{
    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    public byte Nivel { get; set; }

    [StringLength(20)]
    public string Color { get; set; } = string.Empty;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
