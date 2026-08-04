using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class ConfiguracionRed : EntityBase
{
    public int KioskoId { get; set; }

    [StringLength(15)]
    public string TipoConexion { get; set; } = string.Empty;

    public bool DHCP { get; set; } = true;

    [StringLength(50)]
    public string? DireccionIP { get; set; }

    [StringLength(50)]
    public string? MascaraSubred { get; set; }

    [StringLength(50)]
    public string? Gateway { get; set; }

    [StringLength(50)]
    public string? DNSPrimario { get; set; }

    [StringLength(50)]
    public string? DNSSecundario { get; set; }

    public int? Puerto { get; set; }

    public Kiosko? Kiosko { get; set; }
}
