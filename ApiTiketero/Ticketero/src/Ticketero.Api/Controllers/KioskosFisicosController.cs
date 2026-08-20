using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketero.Api.Mapping;
using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;
using Ticketero.Domain.Enums;

namespace Ticketero.Api.Controllers;

[ApiController]
[Route("api/kioskos-fisicos")]
[Authorize]
public class KioskosFisicosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public KioskosFisicosController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private string? GetCurrentUserName()
    {
        return User?.FindFirst(ClaimTypes.Name)?.Value
            ?? User?.FindFirst("nombre")?.Value
            ?? User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var kioskos = await _unitOfWork.Kioskos.FindAsync(k => k.Estado);
        var ubicaciones = await _unitOfWork.Ubicaciones.GetAllAsync();
        var impresoras = await _unitOfWork.ConfiguracionesImpresora.GetAllAsync();
        var redes = await _unitOfWork.ConfiguracionesRed.GetAllAsync();
        var kioskoAreas = await _unitOfWork.KioskoAreas.GetAllAsync();
        var activosFijos = await _unitOfWork.ActivosFijos.GetAllAsync();
        var multimediaList = await _unitOfWork.ConfiguracionesMultimedia.FindAsync(m => m.Estado);
        var multimediaPorKiosko = multimediaList.GroupBy(m => m.KioskoId).ToDictionary(g => g.Key, g => g.ToList());

        var result = kioskos.Select(k =>
        {
            var areaIds = kioskoAreas
                .Where(ka => ka.KioskoId == k.Id)
                .Select(ka => ka.AreaId)
                .ToList();
            var mm = multimediaPorKiosko.GetValueOrDefault(k.Id);
            return MappingService.MapToKioskoFisicoResponse(
                k,
                ubicaciones.FirstOrDefault(u => u.Id == k.UbicacionId),
                impresoras.FirstOrDefault(i => i.KioskoId == k.Id),
                redes.FirstOrDefault(r => r.KioskoId == k.Id),
                areaIds,
                activosFijos.Where(a => a.KioskoId == k.Id).ToList(),
                logoUrl: mm?.FirstOrDefault(m => TipoContenidoMultimedia.EsLogo(m.TipoContenido))?.RutaArchivo,
                videoUrl: mm?.FirstOrDefault(m => TipoContenidoMultimedia.EsVideo(m.TipoContenido))?.RutaArchivo);
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var kiosko = await _unitOfWork.Kioskos.GetByIdAsync(id);
        if (kiosko == null) return NotFound();

        var ubicacion = await _unitOfWork.Ubicaciones.GetByIdAsync(kiosko.UbicacionId);
        var impresoras = await _unitOfWork.ConfiguracionesImpresora.FindAsync(i => i.KioskoId == id);
        var redes = await _unitOfWork.ConfiguracionesRed.FindAsync(r => r.KioskoId == id);
        var kioskoAreas = await _unitOfWork.KioskoAreas.FindAsync(ka => ka.KioskoId == id);
        var activosFijos = await _unitOfWork.ActivosFijos.FindAsync(a => a.KioskoId == id);
        var multimedia = await _unitOfWork.ConfiguracionesMultimedia.FindAsync(m => m.KioskoId == id && m.Estado);

        var areaIds = kioskoAreas.Select(ka => ka.AreaId).ToList();

        return Ok(MappingService.MapToKioskoFisicoResponse(
            kiosko,
            ubicacion,
            impresoras.FirstOrDefault(),
            redes.FirstOrDefault(),
            areaIds,
            activosFijos.ToList(),
            logoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsLogo(m.TipoContenido))?.RutaArchivo,
            videoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsVideo(m.TipoContenido))?.RutaArchivo));
    }

    [HttpPost("auto-registrar")]
    [AllowAnonymous]
    public async Task<IActionResult> AutoRegistrar([FromBody] AutoRegistrarRequest request)
    {
        var ipRemota = HttpContext.Connection.RemoteIpAddress?.ToString() ?? request.IpEquipo ?? "";
        var existente = (await _unitOfWork.Kioskos.FindAsync(k =>
            k.IpKiosko == ipRemota || k.IpEquipo == ipRemota)).FirstOrDefault();

        if (existente != null)
        {
            if (request.IpEquipo != null) existente.IpEquipo = request.IpEquipo;
            if (request.MascaraRedEquipo != null) existente.MascaraRedEquipo = request.MascaraRedEquipo;
            if (request.GatewayEquipo != null) existente.GatewayEquipo = request.GatewayEquipo;
            if (request.DnsEquipo != null) existente.DnsEquipo = request.DnsEquipo;
            existente.IpKiosko = ipRemota;
            await _unitOfWork.Kioskos.UpdateAsync(existente);
            await _unitOfWork.SaveChangesAsync();

            var ubi = await _unitOfWork.Ubicaciones.GetByIdAsync(existente.UbicacionId);
            var imp = (await _unitOfWork.ConfiguracionesImpresora.FindAsync(i => i.KioskoId == existente.Id)).FirstOrDefault();
            var red = (await _unitOfWork.ConfiguracionesRed.FindAsync(r => r.KioskoId == existente.Id)).FirstOrDefault();
            var areas = (await _unitOfWork.KioskoAreas.FindAsync(ka => ka.KioskoId == existente.Id)).Select(ka => ka.AreaId).ToList();
            var activos = (await _unitOfWork.ActivosFijos.FindAsync(a => a.KioskoId == existente.Id)).ToList();
            var multimedia = await _unitOfWork.ConfiguracionesMultimedia.FindAsync(m => m.KioskoId == existente.Id && m.Estado);

            return Ok(MappingService.MapToKioskoFisicoResponse(existente, ubi, imp, red, areas, activos,
                logoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsLogo(m.TipoContenido))?.RutaArchivo,
                videoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsVideo(m.TipoContenido))?.RutaArchivo));
        }

        var nuevo = new Kiosko
        {
            Descripcion = $"Kiosko ({ipRemota})",
            Ubicacion = "Sin asignar",
            UbicacionId = 1,
            IpKiosko = ipRemota,
            IpEquipo = request.IpEquipo,
            MascaraRedEquipo = request.MascaraRedEquipo,
            GatewayEquipo = request.GatewayEquipo,
            DnsEquipo = request.DnsEquipo,
            Estado = true
        };
        await _unitOfWork.Kioskos.AddAsync(nuevo);
        await _unitOfWork.SaveChangesAsync();

        return Ok(MappingService.MapToKioskoFisicoResponse(nuevo, null, null, null, new(), null));
    }

    [HttpPost("detectar-ip")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> DetectarIp([FromBody] DetectarIpRequest request)
    {
        var kiosko = await _unitOfWork.Kioskos.GetByIdAsync(request.KioskoId);
        if (kiosko == null) return NotFound(new { mensaje = "Kiosko no encontrado" });

        kiosko.IpKiosko = request.DireccionIP;
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { kiosko.Id, direccionIP = kiosko.IpKiosko });
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Crear([FromBody] KioskoFisicoCreateRequest request)
    {
        if (request.AreaIds == null || request.AreaIds.Count == 0)
            return BadRequest(new { mensaje = "Debe seleccionar al menos un área para el kiosko" });

        var kiosko = new Kiosko
        {
            Descripcion = request.Nombre,
            Ubicacion = request.Ubicacion,
            UbicacionId = request.UbicacionId,
            IpKiosko = request.IpKiosko ?? request.IpEquipo,
            IpEquipo = request.IpEquipo,
            MascaraRedEquipo = request.MascaraRedEquipo,
            GatewayEquipo = request.GatewayEquipo,
            DnsEquipo = request.DnsEquipo
        };
        await _unitOfWork.Kioskos.AddAsync(kiosko);
        await _unitOfWork.SaveChangesAsync();

        foreach (var areaId in request.AreaIds)
        {
            await _unitOfWork.KioskoAreas.AddAsync(new KioskoArea
            {
                KioskoId = kiosko.Id,
                AreaId = areaId
            });
        }

        if (request.NombreImpresora != null)
        {
            await _unitOfWork.ConfiguracionesImpresora.AddAsync(new ConfiguracionImpresora
            {
                KioskoId = kiosko.Id,
                NombreImpresora = request.NombreImpresora,
                Puerto = request.PuertoImpresora,
                DireccionIP = request.IpImpresora,
                TipoConexion = request.TipoConexionImpresora ?? "USB",
                AnchoPapelMM = request.AnchoPapelMM,
                Copias = request.Copias,
                ImpresionAutomatica = request.ImpresionAutomatica
            });
        }

        if (request.TipoConexionRed != null)
        {
            await _unitOfWork.ConfiguracionesRed.AddAsync(new ConfiguracionRed
            {
                KioskoId = kiosko.Id,
                TipoConexion = request.TipoConexionRed,
                DHCP = request.DHCP,
                DireccionIP = request.DireccionIP,
                MascaraSubred = request.MascaraSubred,
                Gateway = request.GatewayRed,
                DNSPrimario = request.DnsPrimario,
                DNSSecundario = request.DnsSecundario,
                Puerto = request.PuertoRed
            });
        }

        if (request.ActivosFijos != null)
        {
            foreach (var af in request.ActivosFijos)
            {
                if (!Ticketero.Domain.Enums.TipoActivoValidator.EsValido(af.TipoActivo))
                    return BadRequest(new { mensaje = $"TipoActivo '{af.TipoActivo}' no es válido. Valores permitidos: {string.Join(", ", Ticketero.Domain.Enums.TipoActivoValidator.ValoresValidos)}" });

                var userName = GetCurrentUserName();
                var nuevoActivo = new ActivoFijo
                {
                    KioskoId = kiosko.Id,
                    TipoActivo = af.TipoActivo,
                    NumeroActivo = af.NumeroActivo,
                    Descripcion = af.Descripcion,
                    Marca = af.Marca,
                    Modelo = af.Modelo,
                    Serie = af.Serie,
                    CreadoPor = userName,
                    UltimaModificacionPor = userName,
                    FechaModificacion = DateTime.UtcNow
                };
                await _unitOfWork.ActivosFijos.AddAsync(nuevoActivo);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.ActivosFijosAuditoria.AddAsync(new ActivoFijoAuditoria
                {
                    ActivoFijoId = nuevoActivo.Id,
                    KioskoId = kiosko.Id,
                    UsuarioNombre = userName ?? "Sistema",
                    Accion = "CREAR",
                    CambioResumen = $"Activo creado: {af.TipoActivo} #{af.NumeroActivo}",
                    FechaCambio = DateTime.UtcNow
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(request.LogoUrl))
        {
            await _unitOfWork.ConfiguracionesMultimedia.AddAsync(new ConfiguracionMultimedia
            {
                KioskoId = kiosko.Id,
                TipoContenido = "Logo",
                NombreContenido = "Logo",
                RutaArchivo = request.LogoUrl,
                Orden = 1,
                Estado = true
            });
        }
        if (!string.IsNullOrWhiteSpace(request.VideoUrl))
        {
            await _unitOfWork.ConfiguracionesMultimedia.AddAsync(new ConfiguracionMultimedia
            {
                KioskoId = kiosko.Id,
                TipoContenido = "Video",
                NombreContenido = "Video",
                RutaArchivo = request.VideoUrl,
                Orden = 1,
                Estado = true
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var ubicacion = await _unitOfWork.Ubicaciones.GetByIdAsync(kiosko.UbicacionId);
        var impresoras = await _unitOfWork.ConfiguracionesImpresora.FindAsync(i => i.KioskoId == kiosko.Id);
        var redes = await _unitOfWork.ConfiguracionesRed.FindAsync(r => r.KioskoId == kiosko.Id);
        var activosFijos = await _unitOfWork.ActivosFijos.FindAsync(a => a.KioskoId == kiosko.Id);
        var multimedia = await _unitOfWork.ConfiguracionesMultimedia.FindAsync(m => m.KioskoId == kiosko.Id && m.Estado);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = kiosko.Id },
            MappingService.MapToKioskoFisicoResponse(
                kiosko, ubicacion,
                impresoras.FirstOrDefault(),
                redes.FirstOrDefault(),
                request.AreaIds,
                activosFijos.ToList(),
                logoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsLogo(m.TipoContenido))?.RutaArchivo,
                videoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsVideo(m.TipoContenido))?.RutaArchivo));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] KioskoFisicoUpdateRequest request)
    {
        var kiosko = await _unitOfWork.Kioskos.GetByIdAsync(id);
        if (kiosko == null) return NotFound();

        if (request.Nombre != null) kiosko.Descripcion = request.Nombre;
        if (request.Ubicacion != null) kiosko.Ubicacion = request.Ubicacion;
        if (request.UbicacionId.HasValue) kiosko.UbicacionId = request.UbicacionId.Value;
        if (request.Activo.HasValue) kiosko.Estado = request.Activo.Value;
        if (request.IpKiosko != null) kiosko.IpKiosko = request.IpKiosko;

        await _unitOfWork.Kioskos.UpdateAsync(kiosko);

        if (request.AreaIds != null)
        {
            if (request.AreaIds.Count == 0)
                return BadRequest(new { mensaje = "Debe seleccionar al menos un área para el kiosko" });

            var existingAreas = await _unitOfWork.KioskoAreas.FindAsync(ka => ka.KioskoId == id);
            foreach (var ea in existingAreas)
                await _unitOfWork.KioskoAreas.DeleteAsync(ea);

            foreach (var areaId in request.AreaIds)
            {
                await _unitOfWork.KioskoAreas.AddAsync(new KioskoArea
                {
                    KioskoId = id,
                    AreaId = areaId
                });
            }
        }

        var impresora = (await _unitOfWork.ConfiguracionesImpresora.FindAsync(i => i.KioskoId == id)).FirstOrDefault();
        if (request.NombreImpresora != null || impresora != null)
        {
            if (impresora != null)
            {
                if (request.NombreImpresora != null) impresora.NombreImpresora = request.NombreImpresora;
                if (request.PuertoImpresora != null) impresora.Puerto = request.PuertoImpresora;
                if (request.IpImpresora != null) impresora.DireccionIP = request.IpImpresora;
                if (request.TipoConexionImpresora != null) impresora.TipoConexion = request.TipoConexionImpresora;
                if (request.AnchoPapelMM.HasValue) impresora.AnchoPapelMM = request.AnchoPapelMM.Value;
                if (request.Copias.HasValue) impresora.Copias = request.Copias.Value;
                if (request.ImpresionAutomatica.HasValue) impresora.ImpresionAutomatica = request.ImpresionAutomatica.Value;
                await _unitOfWork.ConfiguracionesImpresora.UpdateAsync(impresora);
            }
            else if (request.NombreImpresora != null)
            {
                await _unitOfWork.ConfiguracionesImpresora.AddAsync(new ConfiguracionImpresora
                {
                    KioskoId = id,
                    NombreImpresora = request.NombreImpresora,
                    Puerto = request.PuertoImpresora,
                    DireccionIP = request.IpImpresora,
                    TipoConexion = request.TipoConexionImpresora ?? "USB",
                    AnchoPapelMM = request.AnchoPapelMM ?? 80,
                    Copias = request.Copias ?? 1,
                    ImpresionAutomatica = request.ImpresionAutomatica ?? true
                });
            }
        }

        var red = (await _unitOfWork.ConfiguracionesRed.FindAsync(r => r.KioskoId == id)).FirstOrDefault();
        if (request.TipoConexionRed != null || red != null)
        {
            if (red != null)
            {
                if (request.TipoConexionRed != null) red.TipoConexion = request.TipoConexionRed;
                if (request.DHCP.HasValue) red.DHCP = request.DHCP.Value;
                if (request.DireccionIP != null) red.DireccionIP = request.DireccionIP;
                if (request.MascaraSubred != null) red.MascaraSubred = request.MascaraSubred;
                if (request.GatewayRed != null) red.Gateway = request.GatewayRed;
                if (request.DnsPrimario != null) red.DNSPrimario = request.DnsPrimario;
                if (request.DnsSecundario != null) red.DNSSecundario = request.DnsSecundario;
                if (request.PuertoRed.HasValue) red.Puerto = request.PuertoRed;
                await _unitOfWork.ConfiguracionesRed.UpdateAsync(red);
            }
            else if (request.TipoConexionRed != null)
            {
                await _unitOfWork.ConfiguracionesRed.AddAsync(new ConfiguracionRed
                {
                    KioskoId = id,
                    TipoConexion = request.TipoConexionRed,
                    DHCP = request.DHCP ?? true,
                    DireccionIP = request.DireccionIP,
                    MascaraSubred = request.MascaraSubred,
                    Gateway = request.GatewayRed,
                    DNSPrimario = request.DnsPrimario,
                    DNSSecundario = request.DnsSecundario,
                    Puerto = request.PuertoRed
                });
            }
        }

        if (request.LogoUrl != null)
        {
            var logo = (await _unitOfWork.ConfiguracionesMultimedia
                // Comparación directa a propósito: esto lo traduce EF Core a SQL,
                // donde la colación por defecto ya ignora mayúsculas. Un método
                // propio aquí no sería traducible.
                .FindAsync(m => m.KioskoId == id && m.TipoContenido == "Logo" && m.Estado)).FirstOrDefault();
            if (logo != null)
            {
                logo.RutaArchivo = request.LogoUrl;
                await _unitOfWork.ConfiguracionesMultimedia.UpdateAsync(logo);
            }
            else if (!string.IsNullOrWhiteSpace(request.LogoUrl))
            {
                await _unitOfWork.ConfiguracionesMultimedia.AddAsync(new ConfiguracionMultimedia
                {
                    KioskoId = id,
                    TipoContenido = "Logo",
                    NombreContenido = "Logo",
                    RutaArchivo = request.LogoUrl,
                    Orden = 1,
                    Estado = true
                });
            }
        }
        if (request.VideoUrl != null)
        {
            var video = (await _unitOfWork.ConfiguracionesMultimedia
                // Igual que arriba: comparación traducible a SQL.
                .FindAsync(m => m.KioskoId == id && m.TipoContenido == "Video" && m.Estado)).FirstOrDefault();
            if (video != null)
            {
                video.RutaArchivo = request.VideoUrl;
                await _unitOfWork.ConfiguracionesMultimedia.UpdateAsync(video);
            }
            else if (!string.IsNullOrWhiteSpace(request.VideoUrl))
            {
                await _unitOfWork.ConfiguracionesMultimedia.AddAsync(new ConfiguracionMultimedia
                {
                    KioskoId = id,
                    TipoContenido = "Video",
                    NombreContenido = "Video",
                    RutaArchivo = request.VideoUrl,
                    Orden = 1,
                    Estado = true
                });
            }
        }

        if (request.IpEquipo != null) kiosko.IpEquipo = request.IpEquipo;
        if (request.MascaraRedEquipo != null) kiosko.MascaraRedEquipo = request.MascaraRedEquipo;
        if (request.GatewayEquipo != null) kiosko.GatewayEquipo = request.GatewayEquipo;
        if (request.DnsEquipo != null) kiosko.DnsEquipo = request.DnsEquipo;

        if (request.ActivosFijos != null)
        {
            foreach (var af in request.ActivosFijos)
            {
                if (!Ticketero.Domain.Enums.TipoActivoValidator.EsValido(af.TipoActivo))
                    return BadRequest(new { mensaje = $"TipoActivo '{af.TipoActivo}' no es válido. Valores permitidos: {string.Join(", ", Ticketero.Domain.Enums.TipoActivoValidator.ValoresValidos)}" });
            }

            var existingActivos = await _unitOfWork.ActivosFijos.FindAsync(a => a.KioskoId == id);
            var userName = GetCurrentUserName();
            var incomingKeys = request.ActivosFijos
                .Select(af => $"{af.TipoActivo}|{af.NumeroActivo}")
                .ToHashSet();

            // Eliminar activos que ya no están en la lista
            foreach (var ea in existingActivos)
            {
                var key = $"{ea.TipoActivo}|{ea.NumeroActivo}";
                if (!incomingKeys.Contains(key))
                {
                    await _unitOfWork.ActivosFijosAuditoria.AddAsync(new ActivoFijoAuditoria
                    {
                        ActivoFijoId = ea.Id,
                        KioskoId = id,
                        UsuarioNombre = userName ?? "Sistema",
                        Accion = "ELIMINAR",
                        CambioResumen = $"Activo eliminado: {ea.TipoActivo} #{ea.NumeroActivo}",
                        FechaCambio = DateTime.UtcNow
                    });
                    await _unitOfWork.ActivosFijos.DeleteAsync(ea);
                }
            }

            // Agregar o actualizar activos
            foreach (var af in request.ActivosFijos)
            {
                var existente = existingActivos.FirstOrDefault(ea =>
                    ea.TipoActivo == af.TipoActivo && ea.NumeroActivo == af.NumeroActivo);

                if (existente != null)
                {
                    var cambios = new List<string>();
                    if (existente.Descripcion != af.Descripcion) cambios.Add($"Descripcion: '{existente.Descripcion}' → '{af.Descripcion}'");
                    if (existente.Marca != af.Marca) cambios.Add($"Marca: '{existente.Marca}' → '{af.Marca}'");
                    if (existente.Modelo != af.Modelo) cambios.Add($"Modelo: '{existente.Modelo}' → '{af.Modelo}'");
                    if (existente.Serie != af.Serie) cambios.Add($"Serie: '{existente.Serie}' → '{af.Serie}'");

                    existente.Descripcion = af.Descripcion;
                    existente.Marca = af.Marca;
                    existente.Modelo = af.Modelo;
                    existente.Serie = af.Serie;
                    existente.UltimaModificacionPor = userName;
                    existente.FechaModificacion = DateTime.UtcNow;
                    await _unitOfWork.ActivosFijos.UpdateAsync(existente);

                    if (cambios.Any())
                    {
                        await _unitOfWork.ActivosFijosAuditoria.AddAsync(new ActivoFijoAuditoria
                        {
                            ActivoFijoId = existente.Id,
                            KioskoId = id,
                            UsuarioNombre = userName ?? "Sistema",
                            Accion = "ACTUALIZAR",
                            CambioResumen = string.Join("; ", cambios),
                            FechaCambio = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    var nuevoActivo = new ActivoFijo
                    {
                        KioskoId = id,
                        TipoActivo = af.TipoActivo,
                        NumeroActivo = af.NumeroActivo,
                        Descripcion = af.Descripcion,
                        Marca = af.Marca,
                        Modelo = af.Modelo,
                        Serie = af.Serie,
                        CreadoPor = userName,
                        UltimaModificacionPor = userName,
                        FechaModificacion = DateTime.UtcNow
                    };
                    await _unitOfWork.ActivosFijos.AddAsync(nuevoActivo);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.ActivosFijosAuditoria.AddAsync(new ActivoFijoAuditoria
                    {
                        ActivoFijoId = nuevoActivo.Id,
                        KioskoId = id,
                        UsuarioNombre = userName ?? "Sistema",
                        Accion = "CREAR",
                        CambioResumen = $"Activo creado: {af.TipoActivo} #{af.NumeroActivo}",
                        FechaCambio = DateTime.UtcNow
                    });
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var ubicacion = await _unitOfWork.Ubicaciones.GetByIdAsync(kiosko.UbicacionId);
        var impresoras = await _unitOfWork.ConfiguracionesImpresora.FindAsync(i => i.KioskoId == id);
        var redes = await _unitOfWork.ConfiguracionesRed.FindAsync(r => r.KioskoId == id);
        var kioskoAreas = await _unitOfWork.KioskoAreas.FindAsync(ka => ka.KioskoId == id);
        var areaIds = kioskoAreas.Select(ka => ka.AreaId).ToList();
        var activosFijos = await _unitOfWork.ActivosFijos.FindAsync(a => a.KioskoId == id);
        var multimedia = await _unitOfWork.ConfiguracionesMultimedia.FindAsync(m => m.KioskoId == id && m.Estado);

        return Ok(MappingService.MapToKioskoFisicoResponse(
            kiosko, ubicacion,
            impresoras.FirstOrDefault(),
            redes.FirstOrDefault(),
            areaIds,
            activosFijos.ToList(),
            logoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsLogo(m.TipoContenido))?.RutaArchivo,
            videoUrl: multimedia.FirstOrDefault(m => TipoContenidoMultimedia.EsVideo(m.TipoContenido))?.RutaArchivo));
    }

    [HttpGet("{id}/printer-config")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerConfigImpresora(int id)
    {
        var kiosko = await _unitOfWork.Kioskos.GetByIdAsync(id);
        if (kiosko == null) return NotFound();

        var impresora = (await _unitOfWork.ConfiguracionesImpresora.FindAsync(i => i.KioskoId == id)).FirstOrDefault();
        if (impresora == null)
        {
            return Ok(new PrinterConfigResponse { KioskoId = id, TipoConexion = "DESACTIVADO" });
        }

        return Ok(new PrinterConfigResponse
        {
            KioskoId = impresora.KioskoId,
            TipoConexion = impresora.TipoConexion,
            DireccionIP = impresora.DireccionIP,
            Puerto = impresora.Puerto,
            NombreImpresora = impresora.NombreImpresora,
            AnchoPapelMM = impresora.AnchoPapelMM,
            Copias = impresora.Copias,
            ImpresionAutomatica = impresora.ImpresionAutomatica
        });
    }

    [HttpPut("{id}/printer-config")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> ActualizarConfigImpresora(int id, [FromBody] PrinterConfigUpdateRequest request)
    {
        var kiosko = await _unitOfWork.Kioskos.GetByIdAsync(id);
        if (kiosko == null) return NotFound();

        var impresora = (await _unitOfWork.ConfiguracionesImpresora.FindAsync(i => i.KioskoId == id)).FirstOrDefault();
        if (impresora != null)
        {
            impresora.TipoConexion = request.TipoConexion ?? impresora.TipoConexion;
            impresora.DireccionIP = request.DireccionIP;
            impresora.Puerto = request.Puerto;
            impresora.NombreImpresora = request.NombreImpresora ?? impresora.NombreImpresora;
            impresora.AnchoPapelMM = request.AnchoPapelMM;
            impresora.Copias = request.Copias;
            impresora.ImpresionAutomatica = request.ImpresionAutomatica;
            await _unitOfWork.ConfiguracionesImpresora.UpdateAsync(impresora);
        }
        else
        {
            await _unitOfWork.ConfiguracionesImpresora.AddAsync(new ConfiguracionImpresora
            {
                KioskoId = id,
                TipoConexion = request.TipoConexion ?? "DESACTIVADO",
                DireccionIP = request.DireccionIP,
                Puerto = request.Puerto,
                NombreImpresora = request.NombreImpresora ?? "",
                AnchoPapelMM = request.AnchoPapelMM,
                Copias = request.Copias,
                ImpresionAutomatica = request.ImpresionAutomatica
            });
        }

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
