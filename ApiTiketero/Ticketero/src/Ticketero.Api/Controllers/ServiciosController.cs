using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/areas/{areaId}/servicios")]
[Authorize]
public class ServiciosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiciosController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(int areaId)
    {
        var servicios = await _unitOfWork.Servicios.FindAsync(s => s.AreaId == areaId && s.Estado);
        return Ok(servicios.Select(s => new
        {
            s.Id,
            s.AreaId,
            nombre = s.Descripcion,
            icono = s.Icono ?? "help_outline"
        }).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear(int areaId, [FromBody] ServicioCreateRequest request)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
        if (area == null) return NotFound(new { mensaje = "Area no encontrada" });

        var servicio = new Servicio
        {
            AreaId = areaId,
            Descripcion = request.Nombre,
            Icono = request.Icono ?? "help_outline"
        };

        await _unitOfWork.Servicios.AddAsync(servicio);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerTodos), new { areaId }, new
        {
            servicio.Id,
            servicio.AreaId,
            nombre = servicio.Descripcion,
            icono = servicio.Icono
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int areaId, int id, [FromBody] ServicioCreateRequest request)
    {
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
        if (servicio == null || servicio.AreaId != areaId)
            return NotFound(new { mensaje = "Servicio no encontrado" });

        servicio.Descripcion = request.Nombre;
        servicio.Icono = request.Icono ?? servicio.Icono;

        await _unitOfWork.Servicios.UpdateAsync(servicio);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            servicio.Id,
            servicio.AreaId,
            nombre = servicio.Descripcion,
            icono = servicio.Icono
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int areaId, int id)
    {
        var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
        if (servicio == null || servicio.AreaId != areaId)
            return NotFound(new { mensaje = "Servicio no encontrado" });

        servicio.Estado = false;
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}

public class ServicioCreateRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
}
