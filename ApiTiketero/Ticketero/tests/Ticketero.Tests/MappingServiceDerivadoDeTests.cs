using Shouldly;
using Ticketero.Api.Mapping;
using Ticketero.Domain.Entities;

namespace Ticketero.Tests;

public class MappingServiceDerivadoDeTests
{
    private static Ticket CrearTicketMock(int areaOrigenId, string areaOrigenNombre, bool fueDerivado)
    {
        var areaOrigen = new Area { Descripcion = areaOrigenNombre, Prefijo = "TST" };
        var areaDestino = new Area { Descripcion = "Destino", Prefijo = "DST" };

        var atencion = new Atencion
        {
            TicketId = 1,
            AreaId = areaOrigenId,
            Area = areaOrigen,
            AreaDestinoId = fueDerivado ? 999 : null,
            AreaDestino = fueDerivado ? areaDestino : null,
            FechaInicio = DateTime.UtcNow.AddMinutes(-10),
            FueDerivado = fueDerivado,
            TiempoAtencionSegundos = 600
        };

        return new Ticket
        {
            NumeroTicket = "TST-001",
            EstadoTicketId = 5,
            AreaActualId = 999,
            AreaActual = areaDestino,
            FechaCreacion = DateTime.UtcNow.AddMinutes(-15),
            Atenciones = new List<Atencion> { atencion }
        };
    }

    [Fact]
    public void MapToTicketResponse_DerivadoDe_ShouldUseOriginArea_WhenDerived()
    {
        var ticket = CrearTicketMock(areaOrigenId: 10, areaOrigenNombre: "Ventanillas", fueDerivado: true);

        var result = MappingService.MapToTicketResponse(ticket);

        result.DerivadoDe.ShouldBe("10");
        result.DerivadoDeNombre.ShouldBe("Ventanillas");
    }

    [Fact]
    public void MapToTicketResponse_DerivadoDe_ShouldBeNull_WhenNotDerived()
    {
        var ticket = CrearTicketMock(areaOrigenId: 10, areaOrigenNombre: "Ventanillas", fueDerivado: false);

        var result = MappingService.MapToTicketResponse(ticket);

        result.DerivadoDe.ShouldBeNull();
        result.DerivadoDeNombre.ShouldBeNull();
    }

    [Fact]
    public void MapToTicketResponse_DerivadoDe_ShouldNotUseDestinationArea()
    {
        var ticket = CrearTicketMock(areaOrigenId: 10, areaOrigenNombre: "Ventanillas", fueDerivado: true);

        var result = MappingService.MapToTicketResponse(ticket);

        result.DerivadoDe.ShouldNotBe("999");
        result.DerivadoDeNombre.ShouldNotBe("Destino");
    }
}
