using Microsoft.Extensions.DependencyInjection;
using Ticketero.Application.Services;
using Ticketero.Application.UseCases;

namespace Ticketero.Application.Extensions;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICrearTicketUseCase, CrearTicketUseCase>();
        services.AddScoped<IAtenderTicketUseCase, AtenderTicketUseCase>();
        services.AddScoped<ICerrarTicketUseCase, CerrarTicketUseCase>();
        services.AddScoped<IDerivarTicketUseCase, DerivarTicketUseCase>();
        services.AddScoped<ILlamarTicketUseCase, LlamarTicketUseCase>();
        services.AddScoped<IAbrirSesionUseCase, AbrirSesionUseCase>();
        services.AddScoped<ICerrarSesionUseCase, CerrarSesionUseCase>();
        services.AddScoped<JwtService>();

        return services;
    }
}
