using System.ComponentModel.DataAnnotations;

namespace Ticketero.Domain.Entities;

public class Usuario : EntityBase
{
    [StringLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [StringLength(100)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(100)]
    public string CodigoSistema { get; set; } = string.Empty;

    public int RolId { get; set; }

    public int Logs { get; set; }

    public DateTime? FechaUltimoAcceso { get; set; }

    public Rol? Rol { get; set; }
    public ICollection<UsuarioArea> UsuariosArea { get; set; } = new List<UsuarioArea>();
    public ICollection<SesionOperador> Sesiones { get; set; } = new List<SesionOperador>();
    public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
    public ICollection<Marcacion> Marcaciones { get; set; } = new List<Marcacion>();
}
