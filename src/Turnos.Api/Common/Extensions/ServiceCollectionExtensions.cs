using Turnos.Api.Features.Auth.Login;
using Turnos.Api.Features.Auth.Logout;
using Turnos.Api.Features.Auth.RefreshToken;
using Turnos.Api.Features.Auth.RegisterPatient;
using Turnos.Api.Features.Specialties.CreateSpecialty;

namespace Turnos.Api.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTurnosHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreateSpecialtyHandler>();

        services.AddScoped<RegisterPatientHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();

        return services;
    }
}
