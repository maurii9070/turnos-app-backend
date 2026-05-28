using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Specialties.UpdateSpecialty;

public class UpdateSpecialtyEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/specialties/{id:guid}", async (
            Guid id,
            UpdateSpecialtyRequest request,
            UpdateSpecialtyHandler handler,
            IValidator<UpdateSpecialtyRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(id, request, ct);
            return result.Success
                ? Results.Ok(result)
                : result.Message?.Contains("nombre") == true
                    ? Results.Conflict(result)
                    : Results.NotFound(result);
        })
        .WithName("UpdateSpecialty")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.SuperAdmin)))
        .Produces<ApiResponse<UpdateSpecialtyResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
