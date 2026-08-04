using Shouldly;
using Ticketero.Domain.Enums;

namespace Ticketero.Tests;

public class TipoActivoValidatorTests
{
    [Theory]
    [InlineData("Monitor")]
    [InlineData("CPU")]
    [InlineData("Teclado")]
    [InlineData("Mouse")]
    [InlineData("Impresora")]
    [InlineData("Pantalla")]
    [InlineData("LectorCodigoBarras")]
    [InlineData("Camara")]
    [InlineData("Router")]
    [InlineData("UPS")]
    [InlineData("Tablet")]
    [InlineData("Otro")]
    public void EsValido_ShouldReturnTrue_ForValidValues(string tipoActivo)
    {
        TipoActivoValidator.EsValido(tipoActivo).ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("INVALIDO")]
    [InlineData("monitorcito")]
    [InlineData("123")]
    public void EsValido_ShouldReturnFalse_ForInvalidValues(string? tipoActivo)
    {
        TipoActivoValidator.EsValido(tipoActivo!).ShouldBeFalse();
    }

    [Theory]
    [InlineData("monitor")]
    [InlineData("Monitor")]
    [InlineData("MONITOR")]
    public void EsValido_ShouldBeCaseInsensitive(string tipoActivo)
    {
        TipoActivoValidator.EsValido(tipoActivo).ShouldBeTrue();
    }

    [Fact]
    public void ValoresValidos_ShouldContainAllEnumValues()
    {
        var enumNames = Enum.GetValues<TipoActivoEnum>().Select(e => e.ToString());
        foreach (var name in enumNames)
        {
            TipoActivoValidator.ValoresValidos.ShouldContain(name);
        }
    }
}
