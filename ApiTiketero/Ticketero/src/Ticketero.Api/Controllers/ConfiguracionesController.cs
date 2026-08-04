using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ConfiguracionesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfiguracionesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var configs = await _unitOfWork.Configuraciones.GetAllAsync();
        return Ok(configs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var config = await _unitOfWork.Configuraciones.GetByIdAsync(id);
        if (config == null) return NotFound();
        return Ok(config);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] Configuracion config)
    {
        await _unitOfWork.Configuraciones.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = config.Id }, config);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Configuracion config)
    {
        if (id != config.Id) return BadRequest();
        await _unitOfWork.Configuraciones.UpdateAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var config = await _unitOfWork.Configuraciones.GetByIdAsync(id);
        if (config == null) return NotFound();
        config.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}
