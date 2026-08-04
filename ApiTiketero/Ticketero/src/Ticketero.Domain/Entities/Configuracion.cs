using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Configuracion : EntityBase
{
    [StringLength(100)]
    public string Clave { get; set; } = string.Empty;

    [StringLength(500)]
    public string Valor { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }
}
