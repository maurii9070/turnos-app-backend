using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Doctors.Availabilities.ListAvailabilities;

public class ListAvailabilitiesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/doctors/{doctorId:guid}/availabilities", async (
            Guid doctorId,
            DateOnly? from,
            DateOnly? to,
            ListAvailabilitiesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(doctorId, from, to, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("ListDoctorAvailabilities")
        .Produces<ApiResponse<List<ListAvailabilitiesResponse>>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
