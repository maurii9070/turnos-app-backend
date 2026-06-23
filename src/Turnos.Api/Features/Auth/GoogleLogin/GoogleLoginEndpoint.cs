using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Auth.GoogleLogin;

public class GoogleLoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/google", async (
            GoogleLoginRequest request,
            GoogleLoginHandler handler,
            IValidator<GoogleLoginRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var response = await handler.HandleAsync(request, ct);

            if (!response.Success)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(response);
        })
        .WithName("GoogleLogin")
        .RequireRateLimiting("login")
        .Produces<ApiResponse<GoogleLoginResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
