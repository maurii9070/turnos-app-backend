using Microsoft.Extensions.Configuration;

namespace Turnos.Api.Common.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddTurnosCors(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendUrl = configuration["CorsSettings:FrontendUrl"] ?? "http://localhost:3000";

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(frontendUrl)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
