using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Application.UseCases;

public class LlamarTicketUseCase : ILlamarTicketUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public LlamarTicketUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AtenderTicketResponse> EjecutarAsync(int ticketId, int usuarioId, int kioskoId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.FinalizarAtencionesVencidasAsync(ReglasAtencion.MaximoMinutosAtencion);

            var solicitud = await _unitOfWork.Tickets.GetByIdAsync(ticketId)
                ?? throw new InvalidOperationException($"Ticket {ticketId} no encontrado");

            var ticket = solicitud;
            var areaId = solicitud.AreaActualId;

            // Caso 1: si otro operador ya llamó o está atendiendo este ticket,
            // avanzar automáticamente al siguiente ticket pendiente del área.
            if (!TicketEstado.EsEstadoLlamable(solicitud.EstadoTicketId))
            {
                var pendientes = await _unitOfWork.Tickets.GetTicketsPendientesPorAreaAsync(areaId);
                var siguiente = pendientes.FirstOrDefault(t => t.Id != solicitud.Id);

                if (siguiente == null)
                    throw new InvalidOperationException(
                        $"El ticket {solicitud.NumeroTicket} ya fue llamado/atendido y no hay más tickets pendientes por llamar en el área.");

                ticket = await _unitOfWork.Tickets.GetByIdAsync(siguiente.Id) ?? siguiente;
            }

            var ultimoLlamado = await _unitOfWork.Marcaciones.GetUltimoNumeroLlamadoAsync(ticket.Id);
            var nuevoNumero = (byte)(ultimoLlamado < 254 ? ultimoLlamado + 1 : 1);

            ticket.EstadoTicketId = TicketEstado.Asignado;

            var kioskoFinalId = kioskoId;
            if (kioskoFinalId <= 0)
            {
                var activos = await _unitOfWork.Kioskos.FindAsync(k => k.Estado);
                kioskoFinalId = activos.FirstOrDefault()?.Id ?? 1;
            }
            var marcacion = new Marcacion
            {
                TicketId = ticket.Id,
                UsuarioId = usuarioId,
                KioskoId = kioskoFinalId,
                NumeroLlamado = nuevoNumero,
                FechaMarcacion = DateTime.UtcNow
            };

            await _unitOfWork.Marcaciones.AddAsync(marcacion);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return new AtenderTicketResponse
            {
                TicketId = ticket.Id,
                AtencionId = marcacion.Id,
                Mensaje = $"Llamado #{nuevoNumero} realizado para ticket {ticket.NumeroTicket}"
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}