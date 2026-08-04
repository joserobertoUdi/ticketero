using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;

namespace Ticketero.Application.UseCases;

public interface ICerrarSesionUseCase
{
    Task<SesionDto> EjecutarAsync(int sesionOperadorId);
}

public class CerrarSesionUseCase : ICerrarSesionUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public CerrarSesionUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SesionDto> EjecutarAsync(int sesionOperadorId)
    {
        var sesion = await _unitOfWork.SesionesOperador.GetByIdAsync(sesionOperadorId)
            ?? throw new InvalidOperationException("Sesión no encontrada");

        if (sesion.FechaFin != null)
            throw new InvalidOperationException("La sesión ya está cerrada");

        var atencionesActivas = await _unitOfWork.Atenciones.FindAsync(a =>
            a.SesionOperadorId == sesionOperadorId && a.FechaFin == null);
        if (atencionesActivas.Any())
            throw new InvalidOperationException("No se puede cerrar la sesión porque hay atenciones activas sin finalizar");

        // Gap 13: Solo actualizar FechaFin.
        // En la BD, Estado tiene DEFAULT(1) y es flag de existencia lógica del registro.
        // La sesión activa se determina por: WHERE FechaFin IS NULL
        // (ver UX_SesionOperador_Usuario_Activa en SesionOperador_Tabla.sql)
        sesion.FechaFin = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new SesionDto
        {
            SesionOperadorId = sesion.Id,
            FechaInicio = sesion.FechaInicio,
            FechaFin = sesion.FechaFin,
            EstaActiva = false
        };
    }
}
