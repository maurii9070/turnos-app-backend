using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Auth.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh", async (
            RefreshTokenHandler handler,
            IValidator<RefreshTokenRequest> validator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshTokenValue) ||
                string.IsNullOrWhiteSpace(refreshTokenValue))
            {
                return Results.Unauthorized();
            }

            var request = new RefreshTokenRequest(refreshTokenValue);
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var (response, newRefreshToken) = await handler.HandleAsync(request, ct);

            if (!response.Success || newRefreshToken is null)
            {
                httpContext.Response.Cookies.Delete("refreshToken");
                return Results.Unauthorized();
            }

            httpContext.Response.Cookies.Delete("refreshToken");
            httpContext.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(7),
                Path = "/"
            });

            return Results.Ok(response);
        })
        .WithName("RefreshToken")
        .Produces<ApiResponse<RefreshTokenResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
