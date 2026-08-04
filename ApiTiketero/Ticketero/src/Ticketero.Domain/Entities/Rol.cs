using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Rol : EntityBase
{
    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    public int Logs { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
