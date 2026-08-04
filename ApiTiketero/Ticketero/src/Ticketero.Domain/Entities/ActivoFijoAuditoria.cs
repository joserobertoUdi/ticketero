using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class ActivoFijoAuditoria
{
    public int Id { get; set; }
    public int? ActivoFijoId { get; set; }
    public int? KioskoId { get; set; }

    [StringLength(100)]
    public string UsuarioNombre { get; set; } = string.Empty;

    [StringLength(20)]
    public string Accion { get; set; } = string.Empty;

    [StringLength(500)]
    public string? CambioResumen { get; set; }

    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    public ActivoFijo? ActivoFijo { get; set; }
    public Kiosko? Kiosko { get; set; }
}
