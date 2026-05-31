using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Users.GetCurrentUser;

public class GetCurrentUserEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/me", async (
            ClaimsPrincipal user,
            GetCurrentUserHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(user, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("GetCurrentUser")
        .RequireAuthorization()
        .Produces<ApiResponse<GetCurrentUserResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
