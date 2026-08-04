using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Api.Mapping;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/areas")]
[Authorize]
public class AreasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AreasController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] bool? activos, [FromQuery] PagedRequest? paged)
    {
        if (paged != null && paged.PageSize > 0)
        {
            Expression<Func<Area, bool>>? predicate = activos.HasValue ? a => a.Estado == activos.Value : null;
            var result = await _unitOfWork.Areas.GetPagedAsync(paged, predicate);
            return Ok(new PagedResponse<AreaResponse>
            {
                Items = result.Items.Select(MappingService.MapToAreaResponse).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        var areas = activos.HasValue
            ? await _unitOfWork.Areas.FindAsync(a => a.Estado == activos.Value)
            : await _unitOfWork.Areas.GetAllAsync();
        return Ok(areas.Select(MappingService.MapToAreaResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(id);
        if (area == null) return NotFound();
        return Ok(MappingService.MapToAreaResponse(area));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] AreaCreateRequest request)
    {
        var area = new Area
        {
            Descripcion = request.Nombre,
            Prefijo = request.Prefijo,
            LogoUrl = request.LogoUrl
        };
        await _unitOfWork.Areas.AddAsync(area);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = area.Id }, MappingService.MapToAreaResponse(area));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] AreaUpdateRequest request)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(id);
        if (area == null) return NotFound();

        if (request.Nombre != null) area.Descripcion = request.Nombre;
        if (request.Prefijo != null) area.Prefijo = request.Prefijo;
        if (request.LogoUrl != null) area.LogoUrl = request.LogoUrl;
        if (request.Activo.HasValue) area.Estado = request.Activo.Value;

        await _unitOfWork.Areas.UpdateAsync(area);
        await _unitOfWork.SaveChangesAsync();
        return Ok(MappingService.MapToAreaResponse(area));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(id);
        if (area == null) return NotFound();
        area.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/logo")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> SubirLogo(int id, IFormFile archivo)
    {
        var area = await _unitOfWork.Areas.GetByIdAsync(id);
        if (area == null) return NotFound();

        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe seleccionar un archivo");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "areas");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"area_{id}_{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        var logoUrl = $"/uploads/areas/{fileName}";
        area.LogoUrl = logoUrl;
        await _unitOfWork.Areas.UpdateAsync(area);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { logoUrl });
    }
}
