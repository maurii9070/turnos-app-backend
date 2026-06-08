using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Patients.ListPatients;

public class ListPatientsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/patients", async (
            bool? includeInactive,
            ListPatientsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(includeInactive ?? false, ct);
            return Results.Ok(result);
        })
        .WithName("ListPatients")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.SuperAdmin)))
        .Produces<ApiResponse<List<ListPatientsResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
