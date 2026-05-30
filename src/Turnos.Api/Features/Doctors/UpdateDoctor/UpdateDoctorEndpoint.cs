using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Doctors.UpdateDoctor;

public class UpdateDoctorEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/doctors/{id:guid}", async (
            Guid id,
            UpdateDoctorRequest request,
            ClaimsPrincipal user,
            UpdateDoctorHandler handler,
            IValidator<UpdateDoctorRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(id, request, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "No autorizado." => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    "Doctor no encontrado." => Results.NotFound(result),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("UpdateDoctor")
        .RequireAuthorization()
        .Produces<ApiResponse<UpdateDoctorResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
