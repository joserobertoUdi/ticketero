using System.Linq.Expressions;
using Moq;
using Shouldly;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Tests;

public class DerivarTicketUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DerivarTicketUseCase _useCase;

    public DerivarTicketUseCaseTests()
    {
        _useCase = new DerivarTicketUseCase(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task EjecutarAsync_ShouldThrow_WhenAtencionUserNotInAtencionArea()
    {
        var request = new DerivarTicketRequest
        {
            TicketId = 1,
            AtencionId = 1,
            AreaDestinoId = 20
        };

        var atencion = new Atencion
        {
            TicketId = 1,
            UsuarioId = 100,
            AreaId = 10,
            FechaInicio = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1))
            .ReturnsAsync(new Ticket { NumeroTicket = "TST-001", AreaActualId = 10 });
        _unitOfWorkMock.Setup(u => u.Areas.GetByIdAsync(20))
            .ReturnsAsync(new Area { Descripcion = "Destino" });
        _unitOfWorkMock.Setup(u => u.Atenciones.GetByIdAsync(1))
            .ReturnsAsync(atencion);
        _unitOfWorkMock.Setup(u => u.UsuariosArea.FindAsync(It.IsAny<Expression<Func<UsuarioArea, bool>>>()))
            .ReturnsAsync(new List<UsuarioArea>());
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => _useCase.EjecutarAsync(request));

        ex.Message.ShouldContain("no pertenece al área");
    }

    [Fact]
    public async Task EjecutarAsync_ShouldSucceed_WhenAtencionUserBelongsToArea()
    {
        var request = new DerivarTicketRequest
        {
            TicketId = 1,
            AtencionId = 1,
            AreaDestinoId = 20
        };

        var atencion = new Atencion
        {
            TicketId = 1,
            UsuarioId = 100,
            AreaId = 10,
            FechaInicio = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.Tickets.GetByIdAsync(1))
            .ReturnsAsync(new Ticket { NumeroTicket = "TST-001", AreaActualId = 10 });
        _unitOfWorkMock.Setup(u => u.Areas.GetByIdAsync(20))
            .ReturnsAsync(new Area { Descripcion = "Destino" });
        _unitOfWorkMock.Setup(u => u.Atenciones.GetByIdAsync(1))
            .ReturnsAsync(atencion);
        _unitOfWorkMock.Setup(u => u.UsuariosArea.FindAsync(It.IsAny<Expression<Func<UsuarioArea, bool>>>()))
            .ReturnsAsync(new List<UsuarioArea>
            {
                new() { UsuarioId = 100, AreaId = 10 }
            });
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _useCase.EjecutarAsync(request);

        result.Mensaje.ShouldBe("Ticket derivado exitosamente");
    }
}
