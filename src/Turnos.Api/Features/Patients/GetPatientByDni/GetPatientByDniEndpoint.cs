using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Patients.GetPatientByDni;

public class GetPatientByDniEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/patients/by-dni/{dni}", async (
            string dni,
            GetPatientByDniHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(dni, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("GetPatientByDni")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.SuperAdmin)))
        .Produces<ApiResponse<GetPatientByDniResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
