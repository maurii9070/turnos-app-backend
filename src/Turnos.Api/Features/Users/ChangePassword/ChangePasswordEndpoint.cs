using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Users.ChangePassword;

public class ChangePasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/users/me/password", async (
            ChangePasswordRequest request,
            ClaimsPrincipal user,
            ChangePasswordHandler handler,
            IValidator<ChangePasswordRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(request, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "Usuario no encontrado." => Results.NotFound(result),
                    "La contraseña actual es incorrecta." => Results.Json(result, statusCode: StatusCodes.Status400BadRequest),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("ChangePassword")
        .RequireAuthorization()
        .Produces<ApiResponse<ChangePasswordResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
