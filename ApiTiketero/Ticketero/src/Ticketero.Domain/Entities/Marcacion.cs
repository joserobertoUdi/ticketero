namespace Ticketero.Domain.Entities;

public class Marcacion : EntityBase
{
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }
    public int KioskoId { get; set; }
    public byte NumeroLlamado { get; set; }
    public DateTime FechaMarcacion { get; set; } = DateTime.UtcNow;
    public bool Respondio { get; set; }

    public Ticket? Ticket { get; set; }
    public Usuario? Usuario { get; set; }
    public Kiosko? Kiosko { get; set; }
}
