using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Doctors.Appointments.GetDoctorAppointments;

public class GetDoctorAppointmentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/doctors/{doctorId:guid}/appointments", async (
            Guid doctorId,
            DateOnly date,
            GetDoctorAppointmentsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(doctorId, date, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("GetDoctorAppointments")
        .RequireAuthorization()
        .Produces<ApiResponse<List<GetDoctorAppointmentsResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
