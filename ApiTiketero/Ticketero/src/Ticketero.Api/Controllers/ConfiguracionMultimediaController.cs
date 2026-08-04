using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfiguracionMultimediaController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;

    public ConfiguracionMultimediaController(IUnitOfWork unitOfWork, IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _env = env;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? kioskoId)
    {
        if (kioskoId.HasValue)
        {
            var items = await _unitOfWork.ConfiguracionesMultimedia.FindAsync(c => c.KioskoId == kioskoId.Value && c.Estado);
            return Ok(items);
        }
        var todos = await _unitOfWork.ConfiguracionesMultimedia.GetAllAsync();
        return Ok(todos.Where(c => c.Estado));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var item = await _unitOfWork.ConfiguracionesMultimedia.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] ConfiguracionMultimedia config)
    {
        await _unitOfWork.ConfiguracionesMultimedia.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = config.Id }, config);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CargarArchivo(
        [FromForm] int kioskoId,
        [FromForm] string tipoContenido,
        [FromForm] string nombreContenido,
        IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe seleccionar un archivo");

        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "multimedia");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        var config = new ConfiguracionMultimedia
        {
            KioskoId = kioskoId,
            TipoContenido = tipoContenido,
            NombreContenido = nombreContenido,
            RutaArchivo = $"/uploads/multimedia/{fileName}",
            Orden = 1,
            Repetir = true,
            Estado = true
        };

        await _unitOfWork.ConfiguracionesMultimedia.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = config.Id }, config);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ConfiguracionMultimedia config)
    {
        if (id != config.Id) return BadRequest();
        await _unitOfWork.ConfiguracionesMultimedia.UpdateAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var item = await _unitOfWork.ConfiguracionesMultimedia.GetByIdAsync(id);
        if (item == null) return NotFound();
        item.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("kiosko/{kioskoId}/config")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerConfiguracionKiosko(int kioskoId)
    {
        var multimedia = await _unitOfWork.ConfiguracionesMultimedia.FindAsync(c => c.KioskoId == kioskoId && c.Estado);
        var agrupado = new
        {
            KioskoId = kioskoId,
            Videos = multimedia.Where(m => m.TipoContenido == "Video").OrderBy(m => m.Orden).ToList(),
            Logos = multimedia.Where(m => m.TipoContenido == "Logo").OrderBy(m => m.Orden).ToList()
        };
        return Ok(agrupado);
    }
}