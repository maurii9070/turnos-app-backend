using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;

namespace Turnos.Api.Common.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddTurnosDbContext(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TurnosDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());

        return services;
    }
}
