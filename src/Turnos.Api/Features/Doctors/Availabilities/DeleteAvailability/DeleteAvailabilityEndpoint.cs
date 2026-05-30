using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Doctors.Availabilities.DeleteAvailability;

public class DeleteAvailabilityEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/doctors/{doctorId:guid}/availabilities/{availabilityId:guid}", async (
            Guid doctorId,
            Guid availabilityId,
            ClaimsPrincipal user,
            DeleteAvailabilityHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(doctorId, availabilityId, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "No autorizado." => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    _ => Results.NotFound(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("DeleteDoctorAvailability")
        .RequireAuthorization()
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
