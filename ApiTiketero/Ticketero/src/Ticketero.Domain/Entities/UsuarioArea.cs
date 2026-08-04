namespace Ticketero.Domain.Entities;

public class UsuarioArea : EntityBase
{
    public int UsuarioId { get; set; }
    public int AreaId { get; set; }

    public Usuario? Usuario { get; set; }
    public Area? Area { get; set; }
}
