using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Doctors.Schedules.SetSchedules;

public class SetSchedulesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/doctors/{doctorId:guid}/schedules", async (
            Guid doctorId,
            SetSchedulesRequest request,
            ClaimsPrincipal user,
            SetSchedulesHandler handler,
            IValidator<SetSchedulesRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(doctorId, request, user, ct);

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
        .WithName("SetDoctorSchedules")
        .RequireAuthorization()
        .Produces<ApiResponse<List<SetSchedulesResponse>>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
