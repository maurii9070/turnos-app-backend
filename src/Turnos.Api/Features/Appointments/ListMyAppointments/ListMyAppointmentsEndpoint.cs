using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Appointments.ListMyAppointments;

public class ListMyAppointmentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/appointments/me", async (
            ClaimsPrincipal user,
            ListMyAppointmentsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(user, ct);
            return Results.Ok(result);
        })
        .WithName("ListMyAppointments")
        .RequireAuthorization()
        .Produces<ApiResponse<List<ListMyAppointmentsResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
