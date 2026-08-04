namespace Ticketero.Domain.Entities;

public abstract class EntityBase
{
    public int Id { get; protected set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaReg { get; set; } = DateTime.UtcNow;
    public Guid Ride { get; set; } = Guid.NewGuid();
}
