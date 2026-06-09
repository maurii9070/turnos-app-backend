using Microsoft.Extensions.Configuration;

namespace Turnos.Api.Common.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddTurnosCors(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendUrl = configuration["CorsSettings:FrontendUrl"];
        var mercadoPagoFrontendUrl = configuration["MercadoPago:FrontendBaseUrl"];
        
        var origins = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(frontendUrl))
            origins.Add(frontendUrl.TrimEnd('/'));
        
        if (!string.IsNullOrWhiteSpace(mercadoPagoFrontendUrl) && 
            !origins.Contains(mercadoPagoFrontendUrl.TrimEnd('/')))
            origins.Add(mercadoPagoFrontendUrl.TrimEnd('/'));
        
        if (origins.Count == 0)
            origins.Add("http://localhost:3000");

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(origins.ToArray())
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
