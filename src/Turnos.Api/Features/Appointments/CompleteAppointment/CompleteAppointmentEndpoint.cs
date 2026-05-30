using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Appointments.CompleteAppointment;

public class CompleteAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/appointments/{id:guid}/complete", async (
            Guid id,
            ClaimsPrincipal user,
            CompleteAppointmentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "No autorizado." => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("CompleteAppointment")
        .RequireAuthorization()
        .Produces<ApiResponse<CompleteAppointmentResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
