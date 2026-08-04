namespace Ticketero.Domain.Interfaces;

public interface IEntityBase
{
    int Id { get; }
    bool Estado { get; }
    DateTime FechaReg { get; }
    Guid Ride { get; }
}
