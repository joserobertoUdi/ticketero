using System.Collections.Concurrent;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public class CrearTicketUseCase : ICrearTicketUseCase
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> AreaLocks = new();
    private readonly IUnitOfWork _unitOfWork;

    public CrearTicketUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTicketResponse> EjecutarAsync(CreateTicketRequest request)
    {
        var semaphore = AreaLocks.GetOrAdd(request.AreaActualId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var area = await _unitOfWork.Areas.GetByIdAsync(request.AreaActualId);
                var prefijo = area?.Prefijo ?? "GEN";
                var maxTicket = (await _unitOfWork.Tickets.FindAsync(t =>
                    t.AreaActualId == request.AreaActualId))
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefault();

                var correlativo = 1;
                if (maxTicket != null && maxTicket.NumeroTicket.StartsWith(prefijo))
                {
                    var parts = maxTicket.NumeroTicket.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out var lastNum))
                        correlativo = lastNum + 1;
                }

                var numeroTicket = $"{prefijo}-{correlativo:D3}";

                var ticket = new Ticket
                {
                    NumeroTicket = numeroTicket,
                    ServicioId = request.ServicioId,
                    TipoTicketId = request.TipoTicketId,
                    PrioridadId = request.PrioridadId,
                    EstadoTicketId = TicketEstado.Nuevo,
                    AreaActualId = request.AreaActualId,
                    Descripcion = request.Descripcion,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Tickets.AddAsync(ticket);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new CreateTicketResponse
                {
                    TicketId = ticket.Id,
                    NumeroTicket = ticket.NumeroTicket,
                    Mensaje = "Ticket creado exitosamente"
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
