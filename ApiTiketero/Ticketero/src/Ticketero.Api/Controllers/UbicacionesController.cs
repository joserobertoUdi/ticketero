using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UbicacionesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UbicacionesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var ubicaciones = await _unitOfWork.Ubicaciones.FindAsync(u => u.Estado);
        return Ok(ubicaciones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var ubicacion = await _unitOfWork.Ubicaciones.GetByIdAsync(id);
        if (ubicacion == null) return NotFound();
        return Ok(ubicacion);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] Ubicacion ubicacion)
    {
        await _unitOfWork.Ubicaciones.AddAsync(ubicacion);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = ubicacion.Id }, ubicacion);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Ubicacion ubicacion)
    {
        if (id != ubicacion.Id) return BadRequest();
        await _unitOfWork.Ubicaciones.UpdateAsync(ubicacion);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ubicacion = await _unitOfWork.Ubicaciones.GetByIdAsync(id);
        if (ubicacion == null) return NotFound();
        ubicacion.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}
