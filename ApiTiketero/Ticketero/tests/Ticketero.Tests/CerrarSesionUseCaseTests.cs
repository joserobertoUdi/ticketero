using System.Linq.Expressions;
using Moq;
using Shouldly;
using Ticketero.Application.Interfaces;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Tests;

public class CerrarSesionUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CerrarSesionUseCase _useCase;

    public CerrarSesionUseCaseTests()
    {
        _useCase = new CerrarSesionUseCase(_unitOfWorkMock.Object);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenSesionYaCerrada()
    {
        var sesion = new SesionOperador
        {
            FechaInicio = DateTime.UtcNow.AddHours(-1),
            FechaFin = DateTime.UtcNow
        }.WithId(5);
        _unitOfWorkMock.Setup(u => u.SesionesOperador.GetByIdAsync(5)).ReturnsAsync(sesion);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _useCase.EjecutarAsync(5));

        ex.Message.ShouldContain("ya está cerrada");
    }

    [Fact]
    public async Task EjecutarAsync_ShouldFinalizarAtencionesActivas_WhenCerrarVentana()
    {
        // Caso 3: cerrar ventana con atención en curso debe finalizar el
        // ticket, cerrar la atención activa y cerrar la sesión.
        var sesion = new SesionOperador
        {
            UsuarioId = 10,
            FechaInicio = DateTime.UtcNow.AddHours(-1),
            FechaFin = null
        }.WithId(5);
        var atencion = new Atencion
        {
            TicketId = 1,
            UsuarioId = 10,
            SesionOperadorId = 5,
            FechaInicio = DateTime.UtcNow.AddMinutes(-5),
            FechaFin = null,
            EstadoTicketId = TicketEstado.EnAtencion
        }.WithId(1);
        var ticket = new Ticket
        {
            NumeroTicket = "TST-001",
            EstadoTicketId = TicketEstado.EnAtencion
        }.WithId(1);

        _unitOfWorkMock.Setup(u => u.SesionesOperador.GetByIdAsync(5)).ReturnsAsync(sesion);
        _unitOfWorkMock.Setup(u => u.Atenciones.FindAsync(It.IsAny<Expression<Func<Atencion, bool>>>()))
            .ReturnsAsync(new List<Atencion> { atencion });
        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1)).ReturnsAsync(ticket);

        var result = await _useCase.EjecutarAsync(5, "Cierre de ventana");

        result.EstaActiva.ShouldBeFalse();
        result.AtencionesFinalizadas.ShouldBe(1);
        sesion.FechaFin.ShouldNotBeNull();
        atencion.FechaFin.ShouldNotBeNull();
        atencion.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);
        atencion.Observacion!.ShouldContain("Cierre de ventana");
        ticket.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);
        ticket.FechaCierre.ShouldNotBeNull();
    }

    [Fact]
    public async Task EjecutarAsync_ShouldCloseSession_CuandoNoHayAtencionesActivas()
    {
        var sesion = new SesionOperador
        {
            FechaInicio = DateTime.UtcNow.AddHours(-1),
            FechaFin = null
        }.WithId(5);
        _unitOfWorkMock.Setup(u => u.SesionesOperador.GetByIdAsync(5)).ReturnsAsync(sesion);
        _unitOfWorkMock.Setup(u => u.Atenciones.FindAsync(It.IsAny<Expression<Func<Atencion, bool>>>()))
            .ReturnsAsync(new List<Atencion>());

        var result = await _useCase.EjecutarAsync(5);

        result.EstaActiva.ShouldBeFalse();
        result.AtencionesFinalizadas.ShouldBe(0);
        sesion.FechaFin.ShouldNotBeNull();
    }
}