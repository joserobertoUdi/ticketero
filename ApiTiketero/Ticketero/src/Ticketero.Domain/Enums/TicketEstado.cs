namespace Ticketero.Domain.Enums;

public static class TicketEstado
{
    public const int Nuevo = 1;
    public const int Asignado = 2;
    public const int EnAtencion = 3;
    public const int EnEspera = 4;
    public const int Resuelto = 5;
    public const int Cerrado = 6;
    public const int Cancelado = 7;
    public const int Llamado = 8;

    public static readonly HashSet<int> EstadosLlamables = [Nuevo, EnEspera];

    public static bool EsEstadoLlamable(int estadoTicketId) => EstadosLlamables.Contains(estadoTicketId);
}