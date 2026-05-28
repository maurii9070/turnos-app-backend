using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Specialties.GetSpecialties;

public class GetSpecialtiesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/specialties", async (
            GetSpecialtiesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return Results.Ok(result);
        })
            .WithName("GetSpecialties")
            .Produces<ApiResponse<List<GetSpecialtiesResponse>>>(StatusCodes.Status200OK);
    }
}
