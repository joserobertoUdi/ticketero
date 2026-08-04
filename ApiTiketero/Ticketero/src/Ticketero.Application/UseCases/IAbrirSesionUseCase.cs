using Ticketero.Application.DTOs;

namespace Ticketero.Application.UseCases;

public interface IAbrirSesionUseCase
{
    Task<SesionDto> EjecutarAsync(AbrirSesionRequest request);
}
