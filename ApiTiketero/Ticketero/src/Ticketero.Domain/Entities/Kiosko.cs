using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Kiosko : EntityBase
{
    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(150)]
    public string Ubicacion { get; set; } = string.Empty;

    public int UbicacionId { get; set; }

    [StringLength(50)]
    public string? IpKiosko { get; set; }

    [StringLength(50)]
    public string? IpEquipo { get; set; }

    [StringLength(50)]
    public string? MascaraRedEquipo { get; set; }

    [StringLength(50)]
    public string? GatewayEquipo { get; set; }

    [StringLength(50)]
    public string? DnsEquipo { get; set; }

    public Ubicacion? UbicacionNavegacion { get; set; }
    public ICollection<Marcacion> Marcaciones { get; set; } = new List<Marcacion>();
    public ICollection<KioskoArea> KioskoAreas { get; set; } = new List<KioskoArea>();
    public ICollection<ActivoFijo> ActivosFijos { get; set; } = new List<ActivoFijo>();
}
