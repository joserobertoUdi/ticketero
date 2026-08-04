namespace Ticketero.Domain.Entities;

public class KioskoArea : EntityBase
{
    public int KioskoId { get; set; }
    public int AreaId { get; set; }

    public Kiosko? Kiosko { get; set; }
    public Area? Area { get; set; }
}
