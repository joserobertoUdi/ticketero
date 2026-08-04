using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Ticketero.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IWebHostEnvironment env)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operación no válida");
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, env);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Conflicto de concurrencia al actualizar datos");
            await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict, env);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al guardar en la base de datos");
            var mensaje = env.IsDevelopment()
                ? $"Error de base de datos: {ex.InnerException?.Message ?? ex.Message}"
                : "Error al procesar la solicitud. Intente nuevamente.";
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var response = new { error = true, mensaje, codigo = 400 };
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(json);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado");
            await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, env);
        }
        catch (TaskCanceledException)
        {
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno del servidor");
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, env);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, IWebHostEnvironment env)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var mensaje = env.IsDevelopment()
            ? exception.Message
            : statusCode switch
            {
                HttpStatusCode.BadRequest => "La solicitud no es válida",
                HttpStatusCode.Unauthorized => "No autorizado",
                HttpStatusCode.InternalServerError => "Ha ocurrido un error interno. Intente nuevamente más tarde.",
                _ => "Ha ocurrido un error"
            };

        var response = new
        {
            error = true,
            mensaje,
            codigo = (int)statusCode
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}