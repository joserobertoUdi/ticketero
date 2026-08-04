namespace Ticketero.Application.DTOs;

public class AbrirSesionRequest
{
    public int UsuarioId { get; set; }
    public int PuestoId { get; set; }
}

public class CerrarSesionRequest
{
    public int SesionOperadorId { get; set; }
}

public class SesionDto
{
    public int SesionOperadorId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool EstaActiva { get; set; }
}
