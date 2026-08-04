using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class ActivoFijo : EntityBase
{
    public int KioskoId { get; set; }

    [StringLength(50)]
    public string TipoActivo { get; set; } = string.Empty;

    [StringLength(50)]
    public string NumeroActivo { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    [StringLength(100)]
    public string? Marca { get; set; }

    [StringLength(100)]
    public string? Modelo { get; set; }

    [StringLength(100)]
    public string? Serie { get; set; }

    [StringLength(100)]
    public string? CreadoPor { get; set; }

    [StringLength(100)]
    public string? UltimaModificacionPor { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public Kiosko? Kiosko { get; set; }
    public ICollection<ActivoFijoAuditoria> Auditorias { get; set; } = new List<ActivoFijoAuditoria>();
}
