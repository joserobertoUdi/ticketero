using Moq;
using Shouldly;
using Ticketero.Application.Interfaces;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Tests;

public class LlamarTicketUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly LlamarTicketUseCase _useCase;

    public LlamarTicketUseCaseTests()
    {
        _useCase = new LlamarTicketUseCase(_unitOfWorkMock.Object);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.Marcaciones.GetUltimoNumeroLlamadoAsync(It.IsAny<int>()))
            .ReturnsAsync((byte)0);
        _unitOfWorkMock.Setup(u => u.Marcaciones.AddAsync(It.IsAny<Marcacion>()))
            .ReturnsAsync((Marcacion m) => m);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldSetAsignado_WhenTicketLlamable()
    {
        var ticket = new Ticket
        {
            NumeroTicket = "TST-001",
            EstadoTicketId = TicketEstado.Nuevo,
            AreaActualId = 10
        }.WithId(1);
        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1)).ReturnsAsync(ticket);

        var result = await _useCase.EjecutarAsync(1, 100, 1);

        result.TicketId.ShouldBe(1);
        ticket.EstadoTicketId.ShouldBe(TicketEstado.Asignado);
        result.Mensaje.ShouldContain("TST-001");
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenTicketNoEncontrado()
    {
        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(999)).ReturnsAsync((Ticket?)null);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _useCase.EjecutarAsync(999, 100, 1));

        ex.Message.ShouldContain("no encontrado");
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldAvanzarAlSiguiente_WhenTicketYaLlamado()
    {
        // Caso 1: otro operador llamó este ticket antes; el segundo operador
        // debe recibir el siguiente ticket disponible del área.
        var ticketSolicitado = new Ticket
        {
            NumeroTicket = "TST-001",
            EstadoTicketId = TicketEstado.Asignado,
            AreaActualId = 10
        }.WithId(1);
        var siguiente = new Ticket
        {
            NumeroTicket = "TST-002",
            EstadoTicketId = TicketEstado.Nuevo,
            AreaActualId = 10,
            Prioridad = new Prioridad { Nivel = 1 }
        }.WithId(2);

        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1)).ReturnsAsync(ticketSolicitado);
        _unitOfWorkMock.Setup(u => u.Tickets.GetTicketsPendientesPorAreaAsync(10))
            .ReturnsAsync(new List<Ticket> { siguiente });

        var result = await _useCase.EjecutarAsync(1, 100, 1);

        result.TicketId.ShouldBe(2);
        siguiente.EstadoTicketId.ShouldBe(TicketEstado.Asignado);
        result.Mensaje.ShouldContain("TST-002");
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenTicketYaLlamadoYSinSiguientes()
    {
        var ticketSolicitado = new Ticket
        {
            NumeroTicket = "TST-001",
            EstadoTicketId = TicketEstado.Asignado,
            AreaActualId = 10
        }.WithId(1);
        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1)).ReturnsAsync(ticketSolicitado);
        _unitOfWorkMock.Setup(u => u.Tickets.GetTicketsPendientesPorAreaAsync(10))
            .ReturnsAsync(new List<Ticket>());

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _useCase.EjecutarAsync(1, 100, 1));

        ex.Message.ShouldContain("no hay más tickets pendientes");
    }

    [Fact]
    public async Task EjecutarAsync_ShouldLlamarMismoTicket_WhenEnEspera()
    {
        var ticket = new Ticket
        {
            NumeroTicket = "TST-003",
            EstadoTicketId = TicketEstado.EnEspera,
            AreaActualId = 10
        }.WithId(3);
        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(3)).ReturnsAsync(ticket);

        var result = await _useCase.EjecutarAsync(3, 100, 1);

        result.TicketId.ShouldBe(3);
        ticket.EstadoTicketId.ShouldBe(TicketEstado.Asignado);
    }
}