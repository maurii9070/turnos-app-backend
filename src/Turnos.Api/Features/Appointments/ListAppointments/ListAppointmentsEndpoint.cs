using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.ListAppointments;

public class ListAppointmentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/appointments", async (
            string? status,
            ListAppointmentsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(status, ct);
            return Results.Ok(result);
        })
        .WithName("ListAppointments")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.SuperAdmin)))
        .Produces<ApiResponse<List<ListAppointmentsResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
