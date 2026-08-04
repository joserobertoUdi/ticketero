using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Application.Services;
using Ticketero.Application.UseCases;
using Ticketero.Domain.Entities;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAbrirSesionUseCase _abrirSesion;
    private readonly ICerrarSesionUseCase _cerrarSesion;
    private readonly JwtService _jwtService;

    public UsuariosController(
        IUnitOfWork unitOfWork,
        IAbrirSesionUseCase abrirSesion,
        ICerrarSesionUseCase cerrarSesion,
        JwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _abrirSesion = abrirSesion;
        _cerrarSesion = cerrarSesion;
        _jwtService = jwtService;
    }

    private async Task<IActionResult> EjecutarConTransaccion(Func<Task<IActionResult>> accion)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await accion();
            await _unitOfWork.CommitTransactionAsync();
            return result;
        }
        catch (InvalidOperationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> ObtenerTodos()
    {
        var usuarios = await _unitOfWork.Usuarios.FindAsync(u => u.Estado);
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });
        return Ok(usuario);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request)
    {
        var existe = await _unitOfWork.Usuarios.GetByCorreoAsync(request.Correo);
        if (existe != null)
            return Conflict(new { mensaje = "El correo ya está registrado" });

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CodigoSistema = request.CodigoSistema,
            RolId = request.RolId
        };

        await _unitOfWork.Usuarios.AddAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario);
    }

    [Obsolete("Usar POST /api/auth/login en su lugar")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _unitOfWork.Usuarios.GetByCorreoAsync(request.Correo);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            return Unauthorized(new LoginResponse { Exitoso = false, Mensaje = "Credenciales inválidas" });
        if (!usuario.Estado)
            return Unauthorized(new LoginResponse { Exitoso = false, Mensaje = "Usuario inactivo" });

        var rol = await _unitOfWork.Roles.GetByIdAsync(usuario.RolId);
        var token = _jwtService.GenerarToken(usuario.Id, usuario.Correo, rol?.Descripcion ?? "");

        return Ok(new LoginResponse
        {
            Exitoso = true,
            Mensaje = "Inicio de sesión exitoso",
            Token = token,
            Usuario = new UsuarioDto
            {
                UsuarioId = usuario.Id,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Correo = usuario.Correo,
                CodigoSistema = usuario.CodigoSistema,
                Rol = rol?.Descripcion ?? "",
                Estado = usuario.Estado
            }
        });
    }

    [HttpPost("{id}/sesion")]
    [Authorize]
    public Task<IActionResult> AbrirSesion(int id, [FromBody] AbrirSesionRequest request)
    {
        request.UsuarioId = id;
        return EjecutarConTransaccion(async () =>
        {
            var sesion = await _abrirSesion.EjecutarAsync(request);
            return Ok(sesion);
        });
    }

    [HttpPost("sesion/{sesionId}/cerrar")]
    [Authorize]
    public async Task<IActionResult> CerrarSesion(int sesionId)
    {
        var sesion = await _cerrarSesion.EjecutarAsync(sesionId);
        return Ok(sesion);
    }
}
