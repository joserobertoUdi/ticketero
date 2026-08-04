using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class ConfiguracionMultimedia : EntityBase
{
    public int KioskoId { get; set; }

    [StringLength(200)]
    public string NombreContenido { get; set; } = string.Empty;

    [StringLength(20)]
    public string TipoContenido { get; set; } = string.Empty;

    [StringLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;

    public int? DuracionSegundos { get; set; }

    public int Orden { get; set; } = 1;

    public bool Repetir { get; set; } = true;

    public Kiosko? Kiosko { get; set; }
}
