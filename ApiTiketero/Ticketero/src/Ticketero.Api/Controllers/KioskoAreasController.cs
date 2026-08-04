using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KioskoAreasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public KioskoAreasController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? kioskoId)
    {
        if (kioskoId.HasValue)
        {
            var items = await _unitOfWork.KioskoAreas.FindAsync(ka => ka.KioskoId == kioskoId.Value);
            return Ok(items);
        }
        var todos = await _unitOfWork.KioskoAreas.GetAllAsync();
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var item = await _unitOfWork.KioskoAreas.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] KioskoArea kioskoArea)
    {
        await _unitOfWork.KioskoAreas.AddAsync(kioskoArea);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = kioskoArea.Id }, kioskoArea);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] KioskoArea kioskoArea)
    {
        if (id != kioskoArea.Id) return BadRequest();
        await _unitOfWork.KioskoAreas.UpdateAsync(kioskoArea);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var item = await _unitOfWork.KioskoAreas.GetByIdAsync(id);
        if (item == null) return NotFound();
        item.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}
