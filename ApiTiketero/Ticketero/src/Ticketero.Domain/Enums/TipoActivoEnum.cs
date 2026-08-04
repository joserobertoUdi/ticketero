namespace Ticketero.Domain.Enums;

public enum TipoActivoEnum
{
    Monitor = 1,
    CPU = 2,
    Teclado = 3,
    Mouse = 4,
    Impresora = 5,
    Pantalla = 6,
    LectorCodigoBarras = 7,
    Camara = 8,
    Router = 9,
    UPS = 10,
    Tablet = 11,
    Otro = 99
}

public static class TipoActivoValidator
{
    public static readonly HashSet<string> ValoresValidos = new(
        Enum.GetValues<TipoActivoEnum>()
            .Select(e => e.ToString()),
        StringComparer.OrdinalIgnoreCase);

    public static bool EsValido(string tipoActivo) =>
        !string.IsNullOrWhiteSpace(tipoActivo) && ValoresValidos.Contains(tipoActivo);
}
