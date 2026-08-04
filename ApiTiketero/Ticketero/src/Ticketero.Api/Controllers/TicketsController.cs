using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Ticketero.Api.Hubs;
using Ticketero.Api.Mapping;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]  // Gap 6: todos los endpoints requieren autenticación por defecto
public class TicketsController : ControllerBase
{
    private readonly ICrearTicketUseCase _crearTicket;
    private readonly IAtenderTicketUseCase _atenderTicket;
    private readonly ICerrarTicketUseCase _cerrarTicket;
    private readonly IDerivarTicketUseCase _derivarTicket;
    private readonly ILlamarTicketUseCase _llamarTicket;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<TicketHub> _hubContext;

    public TicketsController(
        ICrearTicketUseCase crearTicket,
        IAtenderTicketUseCase atenderTicket,
        ICerrarTicketUseCase cerrarTicket,
        IDerivarTicketUseCase derivarTicket,
        ILlamarTicketUseCase llamarTicket,
        IUnitOfWork unitOfWork,
        IHubContext<TicketHub> hubContext)
    {
        _crearTicket = crearTicket;
        _atenderTicket = atenderTicket;
        _cerrarTicket = cerrarTicket;
        _derivarTicket = derivarTicket;
        _llamarTicket = llamarTicket;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    // Gap 6: Creación de tickets desde kiosko físico — sin autenticación (pantalla táctil pública)
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CreateTicketFrontendRequest request)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(request.AreaId);
        if (area == null)
            return BadRequest(new { mensaje = "Área no encontrada" });

        var servicioId = request.ServicioId;
        if (servicioId.HasValue)
        {
            var servicio = await _unitOfWork.Servicios.GetByIdAsync(servicioId.Value);
            if (servicio == null || servicio.AreaId != request.AreaId)
                return BadRequest(new { mensaje = "Servicio no válido para el área seleccionada" });
        }
        else
        {
            var servicios = await _unitOfWork.Servicios.FindAsync(s => s.AreaId == request.AreaId);
            var primerServicio = servicios.FirstOrDefault();
            if (primerServicio == null)
                return BadRequest(new { mensaje = "Área sin servicios configurados" });
            servicioId = primerServicio.Id;
        }

        var dtoRequest = new CreateTicketRequest
        {
            ServicioId = servicioId.Value,
            TipoTicketId = request.TipoTicketId,
            PrioridadId = request.PrioridadId,
            AreaActualId = request.AreaId,
            Descripcion = request.Descripcion ?? $"Ticket generado desde kiosko para área {area.Descripcion}"
        };

        var response = await _crearTicket.EjecutarAsync(dtoRequest);
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(response.TicketId);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = response.TicketId },
            ticket != null ? MappingService.MapToTicketResponse(ticket) : null);
    }

    // Gap 6 + Gap 9: Alias obsoleto — usar POST /api/tickets directamente
    [AllowAnonymous]
    [HttpPost("kiosko")]
    [Obsolete("Usar POST /api/tickets (mismo comportamiento). Este alias será removido en la siguiente versión.")]
    public async Task<IActionResult> CrearDesdeKiosko([FromBody] CreateTicketFrontendRequest request)
    {
        Response.Headers.Append("Deprecation", "true");
        Response.Headers.Append("Link", "/api/tickets; rel=\"successor-version\"");
        return await Crear(request);
    }

    // Gap 6: obtener ticket por ID es público (pantalla llamadora, kiosko)
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);
        if (ticket == null) return NotFound(new { mensaje = "Ticket no encontrado" });
        return Ok(MappingService.MapToTicketResponse(ticket));
    }

    // Gap 6: pantalla llamadora y kiosko necesitan ver pendientes sin token
    [AllowAnonymous]
    [HttpGet("pendientes")]
    [Obsolete("Usar GET /api/tickets/pending?areaId=X (más flexible). Será removido en la siguiente versión.")]
    public async Task<IActionResult> ObtenerPendientes()
    {
        Response.Headers.Append("Deprecation", "true");
        Response.Headers.Append("Link", "/api/tickets/pending; rel=\"successor-version\"");
        var tickets = await _unitOfWork.Tickets.GetTicketsPendientesAsync();
        return Ok(new PendingTicketsResponse
        {
            Tickets = tickets.Select(MappingService.MapToTicketResponse).ToList()
        });
    }

    // Gap 6: pantalla llamadora necesita polling sin auth token
    [AllowAnonymous]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int? areaId)
    {
        var tickets = areaId.HasValue
            ? await _unitOfWork.Tickets.GetTicketsPendientesPorAreaAsync(areaId.Value)
            : await _unitOfWork.Tickets.GetTicketsPendientesAsync();

        return Ok(new PendingTicketsResponse
        {
            Tickets = tickets.Select(MappingService.MapToTicketResponse).ToList()
        });
    }

    [HttpPost("{id}/call")]
    public async Task<IActionResult> Call(int id, [FromBody] CallTicketRequest request)
    {
        var response = await _llamarTicket.EjecutarAsync(id, request.UserId, request.KioskoId);
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);
        return Ok(ticket != null ? MappingService.MapToTicketResponse(ticket) : null);
    }

    [HttpPost("{id}/start-attention")]
    public async Task<IActionResult> StartAttention(int id, [FromBody] StartAttentionRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var atencionActiva = await _unitOfWork.Atenciones.GetAtencionActivaPorTicketAsync(id);
            if (atencionActiva != null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest(new { mensaje = "El ticket ya tiene una atención activa" });
            }

            var sesion = await _unitOfWork.SesionesOperador.GetSesionActivaPorUsuarioAsync(request.UserId);

            if (sesion == null)
            {
                if (request.PuestoId <= 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { mensaje = "Debe seleccionar un puesto antes de iniciar atención" });
                }

                var puestoValido = await _unitOfWork.Puestos.GetByIdAsync(request.PuestoId);
                if (puestoValido == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { mensaje = "El puesto especificado no existe" });
                }

                sesion = new SesionOperador
                {
                    UsuarioId = request.UserId,
                    PuestoId = request.PuestoId,
                    FechaInicio = DateTime.UtcNow
                };
                await _unitOfWork.SesionesOperador.AddAsync(sesion);
                await _unitOfWork.SaveChangesAsync();
            }

            var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);
            if (ticket == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return NotFound(new { mensaje = "Ticket no encontrado" });
            }

            var atencionRequest = new AtenderTicketRequest
            {
                TicketId = id,
                UsuarioId = request.UserId,
                AreaId = ticket.AreaActualId,
                ServicioId = ticket.ServicioId,
                SesionOperadorId = sesion.Id
            };

            await _atenderTicket.EjecutarAsync(atencionRequest);
            await _unitOfWork.CommitTransactionAsync();

            var updatedTicket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);
            return Ok(updatedTicket != null ? MappingService.MapToTicketResponse(updatedTicket) : null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteTicketRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var atenciones = await _unitOfWork.Atenciones.FindAsync(a => a.TicketId == id && a.FechaFin == null);
            var atencion = atenciones.FirstOrDefault();
            if (atencion == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest(new { mensaje = "No hay atención activa para este ticket" });
            }

            var cerrarRequest = new CerrarTicketRequest
            {
                TicketId = id,
                AtencionId = atencion.Id,
                Observacion = request.Observacion
            };

            await _cerrarTicket.EjecutarAsync(cerrarRequest);
            await _unitOfWork.CommitTransactionAsync();

            var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);
            return Ok(ticket != null ? MappingService.MapToTicketResponse(ticket) : null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelTicketRequest request)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
        if (ticket == null) return NotFound(new { mensaje = "Ticket no encontrado" });

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var atencionActiva = await _unitOfWork.Atenciones.GetAtencionActivaPorTicketAsync(id);
            if (atencionActiva != null)
            {
                atencionActiva.FechaFin = DateTime.UtcNow;
                atencionActiva.TiempoAtencionSegundos = (int)(DateTime.UtcNow - atencionActiva.FechaInicio).TotalSeconds;
                atencionActiva.EstadoTicketId = TicketEstado.Cancelado;
                atencionActiva.Observacion = request.Motivo;
            }

            ticket.EstadoTicketId = TicketEstado.Cancelado;
            ticket.FechaCierre = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var updatedTicket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);
            if (updatedTicket != null)
            {
                var responseDto = MappingService.MapToTicketResponse(updatedTicket);
                await _hubContext.Clients.Group($"area_{updatedTicket.AreaActualId}").SendAsync("ReceiveTicketUpdate", responseDto);
                return Ok(responseDto);
            }
            return Ok(null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    [HttpGet("current/{userId}")]
    public async Task<IActionResult> GetCurrent(int userId)
    {
        var ticket = await _unitOfWork.Tickets.GetCurrentTicketByUserAsync(userId);
        if (ticket == null) return NoContent();
        return Ok(MappingService.MapToTicketResponse(ticket));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int userId, [FromQuery] string? fecha)
    {
        DateTime fechaParsed;
        if (!DateTime.TryParse(fecha, out fechaParsed))
            fechaParsed = DateTime.UtcNow.Date;
        else
            fechaParsed = DateTime.SpecifyKind(fechaParsed.Date, DateTimeKind.Utc);

        var tickets = await _unitOfWork.Tickets.GetHistoryAsync(userId, fechaParsed);
        return Ok(new
        {
            tickets = tickets.Select(MappingService.MapToTicketResponse).ToList(),
            total = tickets.Count,
            fecha = fechaParsed.ToString("yyyy-MM-dd")
        });
    }

    // Gap 9: alias legacy — usar POST /{id}/complete
    [HttpPost("{id}/cerrar")]
    [Obsolete("Usar POST /api/tickets/{id}/complete. Será removido en la siguiente versión.")]
    public async Task<IActionResult> Cerrar(int id, [FromBody] CerrarTicketRequest request)
    {
        Response.Headers.Append("Deprecation", "true");
        Response.Headers.Append("Link", $"/api/tickets/{id}/complete; rel=\"successor-version\"");
        request.TicketId = id;
        var response = await _cerrarTicket.EjecutarAsync(request);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id}/attention-history")]
    public async Task<IActionResult> GetAttentionHistory(int id)
    {
        var atenciones = await _unitOfWork.Atenciones.GetAtencionesPorTicketAsync(id);
        if (atenciones == null || !atenciones.Any())
            return NotFound(new { mensaje = "No hay historial de atención para este ticket" });

        var history = atenciones.Select(a => new AttentionHistoryItem
        {
            AtencionId = a.Id,
            UsuarioNombre = a.Usuario != null ? $"{a.Usuario.Nombre} {a.Usuario.Apellido}" : "",
            AreaNombre = a.Area?.Descripcion ?? "",
            ServicioNombre = a.Servicio?.Descripcion ?? "",
            FechaInicio = a.FechaInicio,
            FechaFin = a.FechaFin,
            TiempoAtencionSegundos = a.TiempoAtencionSegundos,
            TiempoAtencionFormateado = a.TiempoAtencionSegundos != null
                ? MappingService.FormatearTiempo(a.TiempoAtencionSegundos.Value)
                : null,
            FueDerivado = a.FueDerivado,
            AreaDestinoNombre = a.AreaDestino?.Descripcion,
            Observacion = a.Observacion
        }).ToList();

        return Ok(history);
    }

    [HttpPost("{id}/derivar")]
    public async Task<IActionResult> Derivar(int id, [FromBody] DerivarTicketRequest request)
    {
        request.TicketId = id;
        if (request.AtencionId <= 0)
        {
            var atencionActiva = (await _unitOfWork.Atenciones
                .FindAsync(a => a.TicketId == id && a.FechaFin == null)).FirstOrDefault();
            if (atencionActiva == null)
                return BadRequest(new { mensaje = "No hay atención activa para este ticket" });
            request.AtencionId = atencionActiva.Id;
        }
        var response = await _derivarTicket.EjecutarAsync(request);
        return Ok(response);
    }

    // Gap 9: alias legacy — usar POST /{id}/call
    [HttpPost("{id}/llamar")]
    [Obsolete("Usar POST /api/tickets/{id}/call. Será removido en la siguiente versión.")]
    public async Task<IActionResult> Llamar(int id, [FromQuery] int usuarioId, [FromQuery] int kioskoId)
    {
        Response.Headers.Append("Deprecation", "true");
        Response.Headers.Append("Link", $"/api/tickets/{id}/call; rel=\"successor-version\"");
        var response = await _llamarTicket.EjecutarAsync(id, usuarioId, kioskoId);
        return Ok(response);
    }
}
