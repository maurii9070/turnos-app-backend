using FluentValidation;
using Microsoft.Extensions.Options;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Common.Security;

namespace Turnos.Api.Features.Auth.GoogleLogin;

public class GoogleLoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/google", async (
            GoogleLoginRequest request,
            GoogleLoginHandler handler,
            IValidator<GoogleLoginRequest> validator,
            IOptions<CookieSettings> cookieOptions,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var (response, refreshToken) = await handler.HandleAsync(request, ct);

            if (!response.Success || refreshToken is null)
            {
                return Results.Unauthorized();
            }

            var cookieSettings = cookieOptions.Value;
            httpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = cookieSettings.GetSameSiteMode(),
                Secure = cookieSettings.Secure,
                MaxAge = TimeSpan.FromDays(7),
                Path = "/"
            });

            return Results.Ok(response);
        })
        .WithName("GoogleLogin")
        .RequireRateLimiting("login")
        .Produces<ApiResponse<GoogleLoginResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
