using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.ListDoctorAppointments;

public class ListDoctorAppointmentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/doctors/me/appointments", async (
            ClaimsPrincipal user,
            ListDoctorAppointmentsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(user, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("ListDoctorAppointments")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Doctor)))
        .Produces<ApiResponse<List<ListDoctorAppointmentsResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}