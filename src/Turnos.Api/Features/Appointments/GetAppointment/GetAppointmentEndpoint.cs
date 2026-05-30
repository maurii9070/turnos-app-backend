using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Appointments.GetAppointment;

public class GetAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/appointments/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            GetAppointmentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, user, ct);

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
        .WithName("GetAppointment")
        .RequireAuthorization()
        .Produces<ApiResponse<GetAppointmentResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
