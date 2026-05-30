using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Doctors.Schedules.GetSchedules;

public class GetSchedulesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/doctors/{doctorId:guid}/schedules", async (
            Guid doctorId,
            GetSchedulesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(doctorId, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("GetDoctorSchedules")
        .Produces<ApiResponse<List<GetSchedulesResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
