namespace Ticketero.Domain.Enums;

/// <summary>
/// Valores válidos de <c>ConfiguracionMultimedia.TipoContenido</c>.
/// <para>
/// La columna es texto libre, así que en la base conviven variantes como
/// <c>video</c> y <c>Video</c> según quién escribiera la fila. Las lecturas se
/// resolvían con <c>== "Video"</c> sobre listas ya materializadas, es decir con
/// LINQ to Objects, que sí distingue mayúsculas — a diferencia de SQL Server,
/// cuya colación por defecto no lo hace. El resultado era que una fila con
/// <c>video</c> existía en la tabla pero la API la devolvía como inexistente,
/// sin ningún error.
/// </para>
/// </summary>
public static class TipoContenidoMultimedia
{
    public const string Logo = "Logo";
    public const string Video = "Video";

    /// <summary>Compara ignorando mayúsculas y espacios sobrantes.</summary>
    public static bool Es(string? valor, string tipo) =>
        string.Equals(valor?.Trim(), tipo, StringComparison.OrdinalIgnoreCase);

    public static bool EsLogo(string? valor) => Es(valor, Logo);

    public static bool EsVideo(string? valor) => Es(valor, Video);
}
