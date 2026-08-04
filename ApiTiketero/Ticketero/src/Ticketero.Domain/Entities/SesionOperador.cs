namespace Ticketero.Domain.Entities;

public class SesionOperador : EntityBase
{
    public int UsuarioId { get; set; }
    public int PuestoId { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFin { get; set; }

    public Usuario? Usuario { get; set; }
    public Puesto? Puesto { get; set; }
    public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
}
