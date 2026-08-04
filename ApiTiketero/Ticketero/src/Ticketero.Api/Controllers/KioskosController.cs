using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KioskosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public KioskosController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var kioskos = await _unitOfWork.Kioskos.GetAllAsync();
        return Ok(kioskos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var kiosko = await _unitOfWork.Kioskos.GetByIdAsync(id);
        if (kiosko == null) return NotFound();
        return Ok(kiosko);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] Kiosko kiosko)
    {
        await _unitOfWork.Kioskos.AddAsync(kiosko);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = kiosko.Id }, kiosko);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] Kiosko kiosko)
    {
        if (id != kiosko.Id) return BadRequest();
        await _unitOfWork.Kioskos.UpdateAsync(kiosko);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var kiosko = await _unitOfWork.Kioskos.GetByIdAsync(id);
        if (kiosko == null) return NotFound();
        kiosko.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

}
