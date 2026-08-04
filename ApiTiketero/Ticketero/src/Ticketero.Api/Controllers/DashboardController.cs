using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Api.Mapping;
using Ticketero.Api.Services;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfExportService _pdfExportService;

    public DashboardController(IUnitOfWork unitOfWork, IPdfExportService pdfExportService)
    {
        _unitOfWork = unitOfWork;
        _pdfExportService = pdfExportService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? fechaInicio, [FromQuery] string? fechaFin)
    {
        var inicio = DateTime.TryParse(fechaInicio, out var fi) ? DateTime.SpecifyKind(fi, DateTimeKind.Utc) : DateTime.UtcNow.Date.AddDays(-30);
        var fin = DateTime.TryParse(fechaFin, out var ff) ? DateTime.SpecifyKind(ff, DateTimeKind.Utc).AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

        var tickets = await _unitOfWork.Tickets.GetTicketsByDateRangeAsync(inicio, fin);
        var totalTickets = tickets.Count;
        var totalAtendidos = tickets.Count(t => t.EstadoTicketId == 6);
        var totalPendientes = tickets.Count(t => t.EstadoTicketId != 6 && t.EstadoTicketId != 7);

        var tiemposAtencion = tickets
            .Select(t => t.Atenciones?.Where(a => a.TiempoAtencionSegundos.HasValue)
                .OrderByDescending(a => a.Id).FirstOrDefault())
            .Where(a => a != null)
            .Select(a => a!.TiempoAtencionSegundos!.Value)
            .ToList();

        var tiemposEspera = new List<int>();
        foreach (var t in tickets)
        {
            var primeraAtencion = t.Atenciones?.OrderBy(a => a.Id).FirstOrDefault();
            if (primeraAtencion?.FechaInicio != null)
            {
                var espera = (int)(primeraAtencion.FechaInicio - t.FechaCreacion).TotalSeconds;
                if (espera >= 0) tiemposEspera.Add(espera);
            }
        }

        var promedioAtencion = tiemposAtencion.Any() ? (int)tiemposAtencion.Average() : 0;
        var promedioEspera = tiemposEspera.Any() ? (int)tiemposEspera.Average() : 0;

        return Ok(new DashboardSummaryResponse
        {
            TotalTickets = totalTickets,
            TotalAtendidos = totalAtendidos,
            TotalPendientes = totalPendientes,
            TiempoPromedioAtencion = promedioAtencion,
            TiempoPromedioAtencionFormateado = MappingService.FormatearTiempo(promedioAtencion),
            TiempoPromedioEspera = promedioEspera,
            TiempoPromedioEsperaFormateado = MappingService.FormatearTiempo(promedioEspera),
            Periodo = new PeriodoInfo
            {
                Inicio = inicio.ToString("o"),
                Fin = fin.ToString("o")
            }
        });
    }

    [HttpGet("by-area")]
    public async Task<IActionResult> GetByArea([FromQuery] string? fechaInicio, [FromQuery] string? fechaFin)
    {
        var inicio = DateTime.TryParse(fechaInicio, out var fi) ? DateTime.SpecifyKind(fi, DateTimeKind.Utc) : DateTime.UtcNow.Date.AddDays(-30);
        var fin = DateTime.TryParse(fechaFin, out var ff) ? DateTime.SpecifyKind(ff, DateTimeKind.Utc).AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

        var tickets = await _unitOfWork.Tickets.GetTicketsByDateRangeAsync(inicio, fin);
        var areas = await _unitOfWork.Areas.GetAllAsync();

        var statsPorArea = areas.Select(area =>
        {
            var ticketsArea = tickets.Where(t => t.AreaActualId == area.Id).ToList();
            var tiempos = ticketsArea
                .SelectMany(t => t.Atenciones ?? new List<Domain.Entities.Atencion>())
                .Where(a => a.TiempoAtencionSegundos.HasValue)
                .Select(a => a.TiempoAtencionSegundos!.Value)
                .ToList();

            return new
            {
                areaId = area.Id,
                areaNombre = area.Descripcion,
                total = ticketsArea.Count,
                atendidos = ticketsArea.Count(t => t.EstadoTicketId == 6),
                pendientes = ticketsArea.Count(t => t.EstadoTicketId != 6 && t.EstadoTicketId != 7),
                tiempoPromedio = tiempos.Any() ? (int)tiempos.Average() : 0
            };
        }).ToList();

        return Ok(new
        {
            areas = statsPorArea,
            total = statsPorArea.Sum(a => a.total)
        });
    }

    [HttpGet("by-user")]
    public async Task<IActionResult> GetByUser([FromQuery] string? fechaInicio, [FromQuery] string? fechaFin)
    {
        var inicio = DateTime.TryParse(fechaInicio, out var fi) ? DateTime.SpecifyKind(fi, DateTimeKind.Utc) : DateTime.UtcNow.Date.AddDays(-30);
        var fin = DateTime.TryParse(fechaFin, out var ff) ? DateTime.SpecifyKind(ff, DateTimeKind.Utc).AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

        var tickets = await _unitOfWork.Tickets.GetTicketsByDateRangeAsync(inicio, fin);
        var atenciones = tickets.SelectMany(t => t.Atenciones ?? new List<Domain.Entities.Atencion>()).ToList();
        var userIds = atenciones.Select(a => a.UsuarioId).Distinct();
        var usuarios = await _unitOfWork.Usuarios.GetAllWithIncludesAsync();

        var statsPorUsuario = userIds.Select(uid =>
        {
            var user = usuarios.FirstOrDefault(u => u.Id == uid);
            var atencionesUser = atenciones.Where(a => a.UsuarioId == uid).ToList();
            var tiempos = atencionesUser
                .Where(a => a.TiempoAtencionSegundos.HasValue)
                .Select(a => a.TiempoAtencionSegundos!.Value)
                .ToList();
            var ticketsAtendidos = atencionesUser.Select(a => a.TicketId).Distinct().Count();

            return new
            {
                userId = uid,
                nombreUsuario = user?.NombreUsuario ?? "",
                nombreCompleto = user != null ? $"{user.Nombre} {user.Apellido}" : "",
                ticketsAtendidos,
                tiempoPromedio = tiempos.Any() ? (int)tiempos.Average() : 0
            };
        }).OrderByDescending(u => u.ticketsAtendidos).ToList();

        return Ok(new
        {
            usuarios = statsPorUsuario,
            total = statsPorUsuario.Sum(u => u.ticketsAtendidos)
        });
    }

    [HttpGet("hourly-breakdown")]
    public async Task<IActionResult> GetHourlyBreakdown([FromQuery] string? fecha)
    {
        var fechaParsed = DateTime.TryParse(fecha, out var f) ? DateTime.SpecifyKind(f.Date, DateTimeKind.Utc) : DateTime.UtcNow.Date;
        var fin = fechaParsed.AddDays(1);

        var tickets = await _unitOfWork.Tickets.GetTicketsByDateRangeAsync(fechaParsed, fin);

        var horas = Enumerable.Range(0, 24).Select(h => new
        {
            hora = h,
            total = tickets.Count(t => t.FechaCreacion.Hour == h),
            atendidos = tickets.Count(t => t.FechaCreacion.Hour == h && t.EstadoTicketId == 6)
        }).ToList();

        return Ok(new
        {
            fecha = fechaParsed.ToString("yyyy-MM-dd"),
            horas,
            total = tickets.Count
        });
    }

    [HttpGet("puestos-status")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> GetPuestosStatus()
    {
        var areas = await _unitOfWork.Areas.FindAsync(a => a.Estado);
        var puestos = await _unitOfWork.Puestos.GetAllAsync();
        var sesionesActivas = (await _unitOfWork.SesionesOperador.FindAsync(s => s.FechaFin == null)).ToList();
        var usuarios = await _unitOfWork.Usuarios.GetAllWithIncludesAsync();
        var atenciones = await _unitOfWork.Atenciones.GetAllAsync();
        var atencionesActivas = atenciones.Where(a => a.FechaFin == null).ToList();

        var result = areas.Select(area =>
        {
            var puestosArea = puestos.Where(p => p.AreaId == area.Id).ToList();
            return new
            {
                areaId = area.Id,
                areaNombre = area.Descripcion,
                areaActiva = area.Estado,
                totalPuestos = puestosArea.Count,
                puestos = puestosArea.Select(puesto =>
                {
                    var sesionActiva = sesionesActivas.FirstOrDefault(s => s.PuestoId == puesto.Id);
                    string status;
                    string? operadorNombre = null;
                    int? operadorId = null;

                    if (!area.Estado)
                    {
                        status = "inhabilitado";
                    }
                    else if (sesionActiva == null)
                    {
                        status = "no_esta";
                    }
                    else
                    {
                        operadorId = sesionActiva.UsuarioId;
                        var user = usuarios.FirstOrDefault(u => u.Id == sesionActiva.UsuarioId);
                        operadorNombre = user != null ? $"{user.Nombre} {user.Apellido}" : null;

                        var tieneAtencionActiva = atencionesActivas.Any(a =>
                            a.UsuarioId == sesionActiva.UsuarioId);
                        status = tieneAtencionActiva ? "atendiendo" : "libre";
                    }

                    return new
                    {
                        puestoId = puesto.Id,
                        nombre = puesto.Descripcion,
                        status,
                        operadorId,
                        operadorNombre
                    };
                }).ToList()
            };
        }).ToList();

        return Ok(new { areas = result });
    }

    [HttpGet("user-stats/{userId}")]
    public async Task<IActionResult> GetUserStats(int userId)
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
        if (inicioSemana > hoy) inicioSemana = inicioSemana.AddDays(-7);
        var finHoy = hoy.AddDays(1);

        var tickets = await _unitOfWork.Tickets.GetTicketsByDateRangeAsync(inicioSemana, finHoy);
        var atenciones = tickets
            .SelectMany(t => t.Atenciones ?? new List<Atencion>())
            .Where(a => a.UsuarioId == userId)
            .ToList();

        var atencionesHoy = atenciones
            .Where(a => a.FechaInicio >= hoy && a.FechaInicio < finHoy)
            .ToList();

        var tiempos = atenciones
            .Where(a => a.TiempoAtencionSegundos.HasValue)
            .Select(a => a.TiempoAtencionSegundos!.Value)
            .ToList();

        var ticketsAtendidos = atenciones.Select(a => a.TicketId).Distinct().Count();
        var ticketsHoy = atencionesHoy.Select(a => a.TicketId).Distinct().Count();

        var statsPorArea = atenciones
            .GroupBy(a => a.AreaId)
            .Select(g =>
            {
                var area = tickets.FirstOrDefault(t => t.AreaActualId == g.Key)?.AreaActual;
                return new
                {
                    areaId = g.Key,
                    areaNombre = area?.Descripcion ?? $"Area #{g.Key}",
                    total = g.Select(a => a.TicketId).Distinct().Count()
                };
            }).ToList();

        return Ok(new
        {
            totalAtendidos = ticketsAtendidos,
            totalHoy = ticketsHoy,
            tiempoPromedioSegundos = tiempos.Any() ? (int)tiempos.Average() : 0,
            tiempoPromedioFormateado = tiempos.Any() ? MappingService.FormatearTiempo((int)tiempos.Average()) : "00:00",
            porArea = statsPorArea
        });
    }

    [HttpGet("export-pdf")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ExportPdf([FromQuery] string? fechaInicio, [FromQuery] string? fechaFin)
    {
        var inicio = DateTime.TryParse(fechaInicio, out var fi) ? DateTime.SpecifyKind(fi, DateTimeKind.Utc) : DateTime.UtcNow.Date.AddDays(-30);
        var fin = DateTime.TryParse(fechaFin, out var ff) ? DateTime.SpecifyKind(ff, DateTimeKind.Utc).AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

        var tickets = await _unitOfWork.Tickets.GetTicketsByDateRangeAsync(inicio, fin);
        var areas = await _unitOfWork.Areas.GetAllAsync();

        var totalTickets = tickets.Count;
        var totalAtendidos = tickets.Count(t => t.EstadoTicketId == 6);
        var totalPendientes = tickets.Count(t => t.EstadoTicketId != 6 && t.EstadoTicketId != 7);

        var tiemposAtencion = tickets
            .SelectMany(t => t.Atenciones ?? new List<Atencion>())
            .Where(a => a.TiempoAtencionSegundos.HasValue)
            .Select(a => a.TiempoAtencionSegundos!.Value)
            .ToList();

        var promedioAtencion = tiemposAtencion.Any() ? (int)tiemposAtencion.Average() : 0;

        var pdfData = new PdfExportData
        {
            Inicio = inicio,
            Fin = fin,
            TotalTickets = totalTickets,
            TotalAtendidos = totalAtendidos,
            TotalPendientes = totalPendientes,
            PromedioAtencion = promedioAtencion,
            StatsPorArea = areas.Select(area =>
            {
                var ticketsArea = tickets.Where(t => t.AreaActualId == area.Id).ToList();
                return new PdfAreaStat
                {
                    AreaNombre = area.Descripcion,
                    Total = ticketsArea.Count,
                    Atendidos = ticketsArea.Count(t => t.EstadoTicketId == 6),
                    Pendientes = ticketsArea.Count(t => t.EstadoTicketId != 6 && t.EstadoTicketId != 7)
                };
            }).ToList()
        };

        var pdf = _pdfExportService.GenerateReport(pdfData);
        return File(pdf, "application/pdf", $"reporte_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf");
    }
}
