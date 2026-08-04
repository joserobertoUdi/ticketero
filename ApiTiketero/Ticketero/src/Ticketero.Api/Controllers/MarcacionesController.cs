using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/marcaciones")]
[Authorize]  // Gap 5: Solo usuarios autenticados pueden operar marcaciones
public class MarcacionesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MarcacionesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("{id}/responder")]
    public async Task<IActionResult> Responder(int id)
    {
        var marcacion = await _unitOfWork.Marcaciones.GetByIdAsync(id);
        if (marcacion == null)
            return NotFound(new { mensaje = "Marcación no encontrada" });

        marcacion.Respondio = true;
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { mensaje = "Llamado respondido exitosamente" });
    }
}
