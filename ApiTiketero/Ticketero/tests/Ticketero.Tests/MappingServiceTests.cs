using Shouldly;
using Ticketero.Api.Mapping;

namespace Ticketero.Tests;

public class MappingServiceTests
{
    [Theory]
    [InlineData("Administrador", "administrador")]
    [InlineData("Supervisor", "supervisor")]
    [InlineData("Operador", "operador")]
    [InlineData("Llamador", "llamador")]
    [InlineData("Desconocido", "desconocido")]
    public void MapRolToFrontend_ShouldMapCorrectly(string rolBd, string expected)
    {
        MappingService.MapRolToFrontend(rolBd).ShouldBe(expected);
    }

    [Theory]
    [InlineData("administrador", "Administrador")]
    [InlineData("supervisor", "Supervisor")]
    [InlineData("operador", "Operador")]
    [InlineData("llamador", "Llamador")]
    [InlineData("inexistente", null)]
    public void MapRolToBd_ShouldMapCorrectly(string? rolFrontend, string? expected)
    {
        MappingService.MapRolToBd(rolFrontend).ShouldBe(expected);
    }

    [Fact]
    public void MapRolToBd_ShouldReturnNull_WhenInputIsNull()
    {
        MappingService.MapRolToBd(null).ShouldBeNull();
    }

    [Theory]
    [InlineData(1, "pendiente")]
    [InlineData(2, "pendiente")]
    [InlineData(3, "en_atencion")]
    [InlineData(4, "pendiente")]
    [InlineData(5, "completado")]
    [InlineData(6, "completado")]
    [InlineData(7, "cancelado")]
    [InlineData(8, "llamado")]
    [InlineData(99, "pendiente")]
    public void MapEstadoToStatus_ShouldMapCorrectly(int estadoTicketId, string expected)
    {
        MappingService.MapEstadoToStatus(estadoTicketId).ShouldBe(expected);
    }

    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(59, "00:59")]
    [InlineData(60, "01:00")]
    [InlineData(3661, "01:01:01")]
    [InlineData(86399, "23:59:59")]
    [InlineData(90000, "25:00:00")]
    public void FormatearTiempo_ShouldFormatCorrectly(int segundos, string expected)
    {
        MappingService.FormatearTiempo(segundos).ShouldBe(expected);
    }
}