using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class TipoTicket : EntityBase
{
    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
