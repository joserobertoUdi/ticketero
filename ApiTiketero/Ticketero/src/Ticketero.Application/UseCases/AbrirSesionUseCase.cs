using Ticketero.Application.DTOs;
using Ticketero.Application.Interfaces;
using Ticketero.Domain.Entities;

namespace Ticketero.Application.UseCases;

public class AbrirSesionUseCase : IAbrirSesionUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public AbrirSesionUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SesionDto> EjecutarAsync(AbrirSesionRequest request)
    {
        if (request.PuestoId <= 0)
            throw new InvalidOperationException("Debe seleccionar un puesto válido");

        var puestoExiste = await _unitOfWork.Puestos.GetByIdAsync(request.PuestoId);
        if (puestoExiste == null || !puestoExiste.Estado)
            throw new InvalidOperationException("El puesto seleccionado no existe o no está disponible");

        var sesionActiva = await _unitOfWork.SesionesOperador.GetSesionActivaPorUsuarioAsync(request.UsuarioId);

        if (sesionActiva != null)
        {
            var atencionesActivas = await _unitOfWork.Atenciones.FindAsync(a =>
                a.SesionOperadorId == sesionActiva.Id && a.FechaFin == null);

            if (atencionesActivas.Any())
                throw new InvalidOperationException(
                    "No se puede cambiar de puesto porque hay atenciones activas sin finalizar. " +
                    "Finalice las atenciones pendientes antes de cambiar de puesto.");

            sesionActiva.FechaFin = DateTime.UtcNow;
        }

        var sesion = new SesionOperador
        {
            UsuarioId = request.UsuarioId,
            PuestoId = request.PuestoId,
            FechaInicio = DateTime.UtcNow
        };

        await _unitOfWork.SesionesOperador.AddAsync(sesion);
        await _unitOfWork.SaveChangesAsync();

        return new SesionDto
        {
            SesionOperadorId = sesion.Id,
            FechaInicio = sesion.FechaInicio,
            EstaActiva = true
        };
    }
}
