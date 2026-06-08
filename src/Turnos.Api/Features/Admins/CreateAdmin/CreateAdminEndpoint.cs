using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Admins.CreateAdmin;

public class CreateAdminEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/admins", async (
            CreateAdminRequest request,
            ClaimsPrincipal user,
            CreateAdminHandler handler,
            IValidator<CreateAdminRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(request, user, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.Conflict(result);
        })
        .WithName("CreateAdmin")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.SuperAdmin)))
        .Produces<ApiResponse<CreateAdminResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
