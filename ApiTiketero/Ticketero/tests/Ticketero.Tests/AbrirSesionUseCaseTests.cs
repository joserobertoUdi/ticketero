using System.Linq.Expressions;
using Moq;
using Shouldly;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Tests;

public class AbrirSesionUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AbrirSesionUseCase _useCase;

    public AbrirSesionUseCaseTests()
    {
        _useCase = new AbrirSesionUseCase(_unitOfWorkMock.Object);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.SesionesOperador.AddAsync(It.IsAny<SesionOperador>()))
            .ReturnsAsync((SesionOperador s) => s);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenPuestoInvalido()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _useCase.EjecutarAsync(new AbrirSesionRequest { UsuarioId = 10, PuestoId = 0 }));

        ex.Message.ShouldContain("puesto");
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenPuestoNoDisponible()
    {
        _unitOfWorkMock.Setup(u => u.Puestos.GetByIdAsync(7))
            .ReturnsAsync(new Puesto { Estado = false }.WithId(7));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _useCase.EjecutarAsync(new AbrirSesionRequest { UsuarioId = 10, PuestoId = 7 }));

        ex.Message.ShouldContain("no está disponible");
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldRecuperarOrfanos_CuandoSesionPreviaConAtencionesActivas()
    {
        // Caso 3: el operador dejó una sesión abierta con atención activa
        // (cierre de ventana sin evento). Al reabrir sesión se finalizan las
        // atenciones huérfanas y se cierra la sesión previa.
        var sesionPrevia = new SesionOperador
        {
            UsuarioId = 10,
            PuestoId = 3,
            FechaInicio = DateTime.UtcNow.AddHours(-2),
            FechaFin = null
        }.WithId(5);
        var atencion = new Atencion
        {
            TicketId = 1,
            UsuarioId = 10,
            SesionOperadorId = 5,
            FechaInicio = DateTime.UtcNow.AddMinutes(-30),
            FechaFin = null,
            EstadoTicketId = TicketEstado.EnAtencion
        }.WithId(1);
        var ticket = new Ticket
        {
            NumeroTicket = "TST-001",
            EstadoTicketId = TicketEstado.EnAtencion
        }.WithId(1);

        _unitOfWorkMock.Setup(u => u.Puestos.GetByIdAsync(7))
            .ReturnsAsync(new Puesto { Estado = true }.WithId(7));
        _unitOfWorkMock.Setup(u => u.SesionesOperador.GetSesionActivaPorUsuarioAsync(10)).ReturnsAsync(sesionPrevia);
        _unitOfWorkMock.Setup(u => u.Atenciones.FindAsync(It.IsAny<Expression<Func<Atencion, bool>>>()))
            .ReturnsAsync(new List<Atencion> { atencion });
        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1)).ReturnsAsync(ticket);

        var result = await _useCase.EjecutarAsync(new AbrirSesionRequest { UsuarioId = 10, PuestoId = 7 });

        result.EstaActiva.ShouldBeTrue();
        sesionPrevia.FechaFin.ShouldNotBeNull();
        atencion.FechaFin.ShouldNotBeNull();
        atencion.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);
        ticket.EstadoTicketId.ShouldBe(TicketEstado.Cerrado);
        ticket.FechaCierre.ShouldNotBeNull();
    }

    [Fact]
    public async Task EjecutarAsync_ShouldOpenSession_CuandoNoHaySesionPrevia()
    {
        _unitOfWorkMock.Setup(u => u.Puestos.GetByIdAsync(7))
            .ReturnsAsync(new Puesto { Estado = true }.WithId(7));
        _unitOfWorkMock.Setup(u => u.SesionesOperador.GetSesionActivaPorUsuarioAsync(10))
            .ReturnsAsync((SesionOperador?)null);

        var result = await _useCase.EjecutarAsync(new AbrirSesionRequest { UsuarioId = 10, PuestoId = 7 });

        result.EstaActiva.ShouldBeTrue();
        _unitOfWorkMock.Verify(u => u.SesionesOperador.AddAsync(It.IsAny<SesionOperador>()), Times.Once);
    }
}