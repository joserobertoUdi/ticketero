using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ConfiguracionImpresoraController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfiguracionImpresoraController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? kioskoId)
    {
        if (kioskoId.HasValue)
        {
            var items = await _unitOfWork.ConfiguracionesImpresora.FindAsync(c => c.KioskoId == kioskoId.Value);
            return Ok(items);
        }
        var todos = await _unitOfWork.ConfiguracionesImpresora.GetAllAsync();
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var item = await _unitOfWork.ConfiguracionesImpresora.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ConfiguracionImpresora config)
    {
        await _unitOfWork.ConfiguracionesImpresora.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = config.Id }, config);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ConfiguracionImpresora config)
    {
        if (id != config.Id) return BadRequest();
        await _unitOfWork.ConfiguracionesImpresora.UpdateAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var item = await _unitOfWork.ConfiguracionesImpresora.GetByIdAsync(id);
        if (item == null) return NotFound();
        item.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}
