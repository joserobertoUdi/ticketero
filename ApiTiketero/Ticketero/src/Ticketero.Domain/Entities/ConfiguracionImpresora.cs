using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class ConfiguracionImpresora : EntityBase
{
    public int KioskoId { get; set; }

    [StringLength(150)]
    public string NombreImpresora { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Puerto { get; set; }

    [StringLength(50)]
    public string? DireccionIP { get; set; }

    [StringLength(20)]
    public string TipoConexion { get; set; } = string.Empty;

    public int AnchoPapelMM { get; set; }

    public byte Copias { get; set; } = 1;

    public bool ImpresionAutomatica { get; set; } = true;

    public Kiosko? Kiosko { get; set; }
}
