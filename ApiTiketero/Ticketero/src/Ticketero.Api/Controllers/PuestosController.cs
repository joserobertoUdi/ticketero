using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/areas/{areaId}/puestos")]
[Authorize]
public class PuestosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PuestosController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int areaId)
    {
        var puestos = await _unitOfWork.Puestos.GetPuestosPorAreaAsync(areaId);
        return Ok(puestos.Select(p => new
        {
            p.Id,
            p.AreaId,
            nombre = p.Descripcion,
            ocupado = p.Sesiones?.Any(s => s.FechaFin == null) ?? false
        }).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(int areaId, [FromBody] PuestoCreateRequest request)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
        if (area == null) return NotFound(new { mensaje = "Area no encontrada" });

        var puesto = new Puesto
        {
            AreaId = areaId,
            Descripcion = request.Nombre
        };

        await _unitOfWork.Puestos.AddAsync(puesto);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { areaId }, new
        {
            puesto.Id,
            puesto.AreaId,
            nombre = puesto.Descripcion,
            ocupado = false
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int areaId, int id, [FromBody] PuestoCreateRequest request)
    {
        var puesto = await _unitOfWork.Puestos.GetByIdAsync(id);
        if (puesto == null || puesto.AreaId != areaId)
            return NotFound(new { mensaje = "Puesto no encontrado" });

        puesto.Descripcion = request.Nombre;
        await _unitOfWork.Puestos.UpdateAsync(puesto);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            puesto.Id,
            puesto.AreaId,
            nombre = puesto.Descripcion,
            ocupado = puesto.Sesiones?.Any(s => s.FechaFin == null) ?? false
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int areaId, int id)
    {
        var puesto = await _unitOfWork.Puestos.GetByIdAsync(id);
        if (puesto == null || puesto.AreaId != areaId)
            return NotFound(new { mensaje = "Puesto no encontrado" });

        var sesionActiva = puesto.Sesiones?.FirstOrDefault(s => s.FechaFin == null);
        if (sesionActiva != null)
            return BadRequest(new { mensaje = "No se puede eliminar un puesto ocupado" });

        puesto.Estado = false;
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}

public class PuestoCreateRequest
{
    public string Nombre { get; set; } = string.Empty;
}
