using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Appointments.CancelAppointment;

public class CancelAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/appointments/{id:guid}/cancel", async (
            Guid id,
            ClaimsPrincipal user,
            CancelAppointmentHandler handler,
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
        .WithName("CancelAppointment")
        .RequireAuthorization()
        .Produces<ApiResponse<CancelAppointmentResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
