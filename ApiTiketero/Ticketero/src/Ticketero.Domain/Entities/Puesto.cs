using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Puesto : EntityBase
{
    public int AreaId { get; set; }

    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    public Area? Area { get; set; }
    public ICollection<SesionOperador> Sesiones { get; set; } = new List<SesionOperador>();
}
