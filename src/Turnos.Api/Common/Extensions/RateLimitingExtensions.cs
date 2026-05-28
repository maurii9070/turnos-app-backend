using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Turnos.Api.Common.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddTurnosRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("login", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            options.OnRejected = (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return new ValueTask();
            };
        });

        return services;
    }
}
