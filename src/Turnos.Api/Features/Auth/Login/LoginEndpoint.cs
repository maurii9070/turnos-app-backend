using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Auth.Login;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (
            LoginRequest request,
            LoginHandler handler,
            IValidator<LoginRequest> validator,
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

            httpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(7),
                Path = "/"
            });

            return Results.Ok(response);
        })
        .WithName("Login")
        .RequireRateLimiting("login")
        .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
