using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Ubicacion : EntityBase
{
    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(100)]
    public string Edificio { get; set; } = string.Empty;

    [StringLength(50)]
    public string Piso { get; set; } = string.Empty;

    [StringLength(100)]
    public string Sector { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Referencia { get; set; }

    public ICollection<Kiosko> Kioskos { get; set; } = new List<Kiosko>();
}
