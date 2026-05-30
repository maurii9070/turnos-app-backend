using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Doctors.ListDoctors;

public class ListDoctorsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/doctors", async (
            Guid? specialtyId,
            ListDoctorsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(specialtyId, ct);
            return Results.Ok(result);
        })
        .WithName("ListDoctors")
        .Produces<ApiResponse<List<ListDoctorsResponse>>>(StatusCodes.Status200OK);
    }
}
