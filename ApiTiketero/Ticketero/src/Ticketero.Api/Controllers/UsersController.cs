using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Api.Mapping;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllWithIncludesAsync();
        var result = new List<UserResponse>();

        foreach (var u in usuarios)
        {
            result.Add(MappingService.MapToUserResponse(u, u.Rol, u.UsuariosArea?.ToList() ?? new()));
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdWithIncludesAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

        return Ok(MappingService.MapToUserResponse(usuario, usuario.Rol, usuario.UsuariosArea?.ToList() ?? new()));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Correo))
        {
            var existe = await _unitOfWork.Usuarios.GetByCorreoAsync(request.Correo);
            if (existe != null)
                return Conflict(new { mensaje = "El correo ya está registrado" });
        }

        var rolBd = MappingService.MapRolToBd(request.Rol) ?? "Operador";
        var roles = await _unitOfWork.Roles.FindAsync(r => r.Descripcion == rolBd);
        var rol = roles.FirstOrDefault();
        if (rol == null)
            return BadRequest(new { mensaje = "Rol inválido" });

        var nombreParts = SplitNombreCompleto(request.NombreCompleto);

        var usuario = new Usuario
        {
            NombreUsuario = request.NombreUsuario,
            Nombre = nombreParts.nombre,
            Apellido = nombreParts.apellido,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CodigoSistema = request.NombreUsuario.ToUpper(),
            RolId = rol.Id
        };

        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        var areasAsignar = request.AreasAtencion?.Any() == true
            ? request.AreasAtencion
            : request.AreaId.HasValue ? new List<int> { request.AreaId.Value } : new List<int>();

        foreach (var areaId in areasAsignar)
        {
            await _unitOfWork.UsuariosArea.AddAsync(new UsuarioArea
            {
                UsuarioId = usuario.Id,
                AreaId = areaId
            });
        }
        if (areasAsignar.Any())
            await _unitOfWork.SaveChangesAsync();

        var areas = (await _unitOfWork.UsuariosArea.FindAsync(ua => ua.UsuarioId == usuario.Id)).ToList();
        var userResponse = MappingService.MapToUserResponse(usuario, rol, areas);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, userResponse);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

        if (request.NombreCompleto != null)
        {
            var parts = SplitNombreCompleto(request.NombreCompleto);
            usuario.Nombre = parts.nombre;
            usuario.Apellido = parts.apellido;
        }

        if (request.Correo != null) usuario.Correo = request.Correo;
        if (request.Password != null) usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        if (request.Activo.HasValue) usuario.Estado = request.Activo.Value;

        if (request.Rol != null)
        {
            var rolBd = MappingService.MapRolToBd(request.Rol);
            if (rolBd != null)
            {
                var roles = await _unitOfWork.Roles.FindAsync(r => r.Descripcion == rolBd);
                var rol = roles.FirstOrDefault();
                if (rol != null) usuario.RolId = rol.Id;
            }
        }

        var areasAAplicar = request.AreasAtencion?.Any() == true
            ? request.AreasAtencion
            : request.AreaId.HasValue ? new List<int> { request.AreaId.Value } : null;

        if (areasAAplicar != null)
        {
            var existingAreas = await _unitOfWork.UsuariosArea.FindAsync(ua => ua.UsuarioId == id);
            foreach (var ea in existingAreas)
            {
                await _unitOfWork.UsuariosArea.DeleteAsync(ea);
            }
            foreach (var areaId in areasAAplicar)
            {
                await _unitOfWork.UsuariosArea.AddAsync(new UsuarioArea
                {
                    UsuarioId = id,
                    AreaId = areaId
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var rolEntity = await _unitOfWork.Roles.GetByIdAsync(usuario.RolId);
        var areas = (await _unitOfWork.UsuariosArea.FindAsync(ua => ua.UsuarioId == usuario.Id)).ToList();
        return Ok(MappingService.MapToUserResponse(usuario, rolEntity, areas));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

        usuario.Estado = false;
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    private static (string nombre, string apellido) SplitNombreCompleto(string nombreCompleto)
    {
        var parts = nombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return ("", "");
        if (parts.Length == 1) return (parts[0], "");
        return (parts[0], string.Join(" ", parts.Skip(1)));
    }
}
