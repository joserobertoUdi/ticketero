using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Area : EntityBase
{
    [StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(100)]
    public string Prefijo { get; set; } = string.Empty;

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    public ICollection<UsuarioArea> UsuariosArea { get; set; } = new List<UsuarioArea>();
    public ICollection<Puesto> Puestos { get; set; } = new List<Puesto>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
