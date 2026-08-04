using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ticketero.Api.Mapping;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Application.Services;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtService _jwtService;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;

    private static readonly TimeSpan RefreshTokenVigencia = TimeSpan.FromDays(30);

    public AuthController(IUnitOfWork unitOfWork, JwtService jwtService, IDistributedCache cache, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _cache = cache;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
    {
        var usuario = await _unitOfWork.Usuarios.GetByCorreoAsync(request.Correo);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            return Unauthorized(new { mensaje = "Credenciales inválidas" });

        if (!usuario.Estado)
            return Unauthorized(new { mensaje = "Usuario inactivo" });

        usuario.FechaUltimoAcceso = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var token = _jwtService.GenerarToken(usuario.Id, usuario.Correo, usuario.Rol?.Descripcion ?? "");
        var rawRefreshToken = _jwtService.GenerarRefreshToken();

        var hashedToken = HashToken(rawRefreshToken);
        await _cache.SetStringAsync(hashedToken, usuario.Id.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = RefreshTokenVigencia
        });

        var userResponse = MappingService.MapToUserResponse(
            usuario, usuario.Rol, usuario.UsuariosArea?.ToList() ?? new());

        var expiryHours = int.TryParse(_configuration["Jwt:ExpiryHours"], out var h) ? h : 1;

        return Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = rawRefreshToken,
            Usuario = userResponse,
            ExpiraEn = DateTime.UtcNow.AddHours(expiryHours).ToString("o")
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] AuthRefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { mensaje = "Se requiere el refresh token" });

        var hashedToken = HashToken(request.RefreshToken);
        var storedUserId = await _cache.GetStringAsync(hashedToken);

        if (storedUserId == null || !int.TryParse(storedUserId, out var usuarioId))
            return Unauthorized(new { mensaje = "Refresh token inválido, expirado o revocado" });

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
        if (usuario == null || !usuario.Estado)
            return Unauthorized(new { mensaje = "Usuario no encontrado o inactivo" });

        await _cache.RemoveAsync(hashedToken);

        var newToken = _jwtService.GenerarToken(usuario.Id, usuario.Correo, usuario.Rol?.Descripcion ?? "");
        var newRawRefreshToken = _jwtService.GenerarRefreshToken();

        var newHashedToken = HashToken(newRawRefreshToken);
        await _cache.SetStringAsync(newHashedToken, usuario.Id.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = RefreshTokenVigencia
        });

        var expiryHours = int.TryParse(_configuration["Jwt:ExpiryHours"], out var h) ? h : 1;

        return Ok(new RefreshTokenResponse
        {
            Token = newToken,
            RefreshToken = newRawRefreshToken,
            ExpiraEn = DateTime.UtcNow.AddHours(expiryHours).ToString("o")
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] AuthRefreshRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var hashedToken = HashToken(request.RefreshToken);
            await _cache.RemoveAsync(hashedToken);
        }
        return Ok(new { mensaje = "Sesión cerrada correctamente" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var usuario = await _unitOfWork.Usuarios.GetByIdWithIncludesAsync(userId);
        if (usuario == null) return NotFound();

        return Ok(MappingService.MapToUserResponse(usuario, usuario.Rol, usuario.UsuariosArea?.ToList() ?? new()));
    }

    /// <summary>Genera un hash SHA-256 del token para almacenamiento seguro en caché.</summary>
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

