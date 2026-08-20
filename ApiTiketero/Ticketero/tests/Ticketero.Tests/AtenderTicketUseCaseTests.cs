using System.Linq.Expressions;
using Moq;
using Shouldly;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Tests;

public class AtenderTicketUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AtenderTicketUseCase _useCase;

    public AtenderTicketUseCaseTests()
    {
        _useCase = new AtenderTicketUseCase(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenUserNotInArea()
    {
        var request = new AtenderTicketRequest
        {
            TicketId = 1,
            UsuarioId = 100,
            AreaId = 10,
            ServicioId = 1,
            SesionOperadorId = 1
        };

        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1))
            .ReturnsAsync(new Ticket { NumeroTicket = "TST-001", EstadoTicketId = TicketEstado.Nuevo });
        _unitOfWorkMock.Setup(u => u.SesionesOperador.GetByIdAsync(1))
            .ReturnsAsync(new SesionOperador());
        _unitOfWorkMock.Setup(u => u.Atenciones.GetAtencionActivaPorTicketAsync(1))
            .ReturnsAsync((Atencion?)null);
        _unitOfWorkMock.Setup(u => u.UsuariosArea.FindAsync(It.IsAny<Expression<Func<UsuarioArea, bool>>>()))
            .ReturnsAsync(new List<UsuarioArea>());

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => _useCase.EjecutarAsync(request));

        ex.Message.ShouldContain("no pertenece al área");
    }

    [Fact]
    public async Task EjecutarAsync_ShouldSucceed_WhenUserInArea()
    {
        var request = new AtenderTicketRequest
        {
            TicketId = 1,
            UsuarioId = 100,
            AreaId = 10,
            ServicioId = 1,
            SesionOperadorId = 1
        };

        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1))
            .ReturnsAsync(new Ticket { NumeroTicket = "TST-001", EstadoTicketId = TicketEstado.Nuevo });
        _unitOfWorkMock.Setup(u => u.SesionesOperador.GetByIdAsync(1))
            .ReturnsAsync(new SesionOperador());
        _unitOfWorkMock.Setup(u => u.Atenciones.GetAtencionActivaPorTicketAsync(1))
            .ReturnsAsync((Atencion?)null);
        _unitOfWorkMock.Setup(u => u.UsuariosArea.FindAsync(It.IsAny<Expression<Func<UsuarioArea, bool>>>()))
            .ReturnsAsync(new List<UsuarioArea>
            {
                new() { UsuarioId = 100, AreaId = 10 }
            });
        _unitOfWorkMock.Setup(u => u.Atenciones.AddAsync(It.IsAny<Atencion>()))
            .ReturnsAsync((Atencion a) => a);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _useCase.EjecutarAsync(request);

        result.Mensaje.ShouldBe("Atención iniciada correctamente");
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenOtroOperadorYaAtiende()
    {
        // Caso 1: el primero que inicia la atención se queda con el ticket.
        // El segundo operador no puede tomar el mismo ticket.
        var request = new AtenderTicketRequest
        {
            TicketId = 1,
            UsuarioId = 100,
            AreaId = 10,
            ServicioId = 1,
            SesionOperadorId = 1
        };

        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1))
            .ReturnsAsync(new Ticket { NumeroTicket = "TST-001", EstadoTicketId = TicketEstado.Asignado });
        _unitOfWorkMock.Setup(u => u.Atenciones.GetAtencionActivaPorTicketAsync(1))
            .ReturnsAsync(new Atencion { TicketId = 1, UsuarioId = 999 });

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => _useCase.EjecutarAsync(request));

        ex.Message.ShouldContain("otro operador");
        _unitOfWorkMock.Verify(u => u.Atenciones.AddAsync(It.IsAny<Atencion>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldBeIdempotente_WhenMismoOperadorYaAtiende()
    {
        // Caso 1: reintento del mismo operador no crea una segunda atención.
        var request = new AtenderTicketRequest
        {
            TicketId = 1,
            UsuarioId = 100,
            AreaId = 10,
            ServicioId = 1,
            SesionOperadorId = 1
        };

        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1))
            .ReturnsAsync(new Ticket { NumeroTicket = "TST-001", EstadoTicketId = TicketEstado.Asignado });
        _unitOfWorkMock.Setup(u => u.Atenciones.GetAtencionActivaPorTicketAsync(1))
            .ReturnsAsync(new Atencion { TicketId = 1, UsuarioId = 100 });

        var result = await _useCase.EjecutarAsync(request);

        result.Mensaje.ShouldContain("Atención ya iniciada");
        _unitOfWorkMock.Verify(u => u.Atenciones.AddAsync(It.IsAny<Atencion>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
