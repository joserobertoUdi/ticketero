using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Api.Mapping;
using Ticketero.Application.Interfaces;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// Gap 14: Catálogos de configuración requieren auth; de selección son públicos para kiosko
public class CatalogosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogosController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Kiosko necesita listar áreas para mostrarlas al usuario
    [AllowAnonymous]
    [HttpGet("areas")]
    public async Task<IActionResult> ObtenerAreas()
    {
        var areas = await _unitOfWork.Areas.FindAsync(a => a.Estado);
        return Ok(areas.Select(MappingService.MapToAreaResponse).ToList());
    }

    // Kiosko necesita listar servicios por área para la selección de tipo de trámite
    [AllowAnonymous]
    [HttpGet("servicios")]
    public async Task<IActionResult> ObtenerServicios()
    {
        var servicios = await _unitOfWork.Servicios.FindAsync(s => s.Estado);
        return Ok(servicios.Select(s => new { s.Id, Nombre = s.Descripcion, s.AreaId, s.Icono }).ToList());
    }

    [AllowAnonymous]
    [HttpGet("tipos-ticket")]
    public async Task<IActionResult> ObtenerTiposTicket()
    {
        var tipos = await _unitOfWork.TiposTicket.GetAllAsync();
        return Ok(tipos.Select(t => new { t.Id, t.Descripcion }).ToList());
    }

    [AllowAnonymous]
    [HttpGet("prioridades")]
    public async Task<IActionResult> ObtenerPrioridades()
    {
        var prioridades = await _unitOfWork.Prioridades.GetAllAsync();
        return Ok(prioridades.Select(p => new { p.Id, p.Descripcion, p.Nivel, p.Color }).ToList());
    }

    [HttpGet("estados-ticket")]
    public async Task<IActionResult> ObtenerEstadosTicket()
    {
        var estados = await _unitOfWork.EstadosTicket.GetAllAsync();
        return Ok(estados.Select(e => new { e.Id, e.Descripcion }).ToList());
    }

    // Roles: solo Administrador puede gestionarlos — requiere auth
    [Authorize(Roles = "Administrador")]
    [HttpGet("roles")]
    public async Task<IActionResult> ObtenerRoles()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return Ok(roles.Select(r => new { Id = r.Id, r.Descripcion }).ToList());
    }

    // Puestos: lo consulta el panel admin y el operador al iniciar sesión — requiere auth
    [Authorize]
    [HttpGet("puestos")]
    public async Task<IActionResult> ObtenerPuestos([FromQuery] int? areaId)
    {
        if (areaId.HasValue)
        {
            var puestos = await _unitOfWork.Puestos.GetPuestosPorAreaAsync(areaId.Value);
            return Ok(puestos.Select(p => new { p.Id, Nombre = p.Descripcion, p.AreaId }).ToList());
        }
        var todos = await _unitOfWork.Puestos.FindAsync(p => p.Estado);
        return Ok(todos.Select(p => new { p.Id, Nombre = p.Descripcion, p.AreaId }).ToList());
    }

    // Kioskos: solo admin puede ver/gestionar la lista de kioskos — requiere auth
    [Authorize(Roles = "Administrador")]
    [HttpGet("kioskos")]
    public async Task<IActionResult> ObtenerKioskos()
    {
        var kioskos = await _unitOfWork.Kioskos.FindAsync(k => k.Estado);
        return Ok(kioskos.Select(k => new { k.Id, Nombre = k.Descripcion, k.Ubicacion, k.IpKiosko }).ToList());
    }
}
