using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Auth.Logout;

public class LogoutEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/logout", async (
            LogoutHandler handler,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshTokenValue) &&
                !string.IsNullOrWhiteSpace(refreshTokenValue))
            {
                await handler.HandleAsync(refreshTokenValue, ct);
            }

            httpContext.Response.Cookies.Delete("refreshToken");

            return Results.Ok(ApiResponse<object>.Ok(new { }, "Sesión cerrada correctamente."));
        })
        .WithName("Logout")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }
}
