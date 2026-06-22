using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Users.LinkGoogle;

public class LinkGoogleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/link-google", async (
            LinkGoogleRequest request,
            LinkGoogleHandler handler,
            IValidator<LinkGoogleRequest> validator,
            ClaimsPrincipal currentUser,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var response = await handler.HandleAsync(request, currentUser, ct);

            if (!response.Success)
            {
                return Results.BadRequest(response);
            }

            return Results.Ok(response);
        })
        .WithName("LinkGoogle")
        .RequireAuthorization()
        .Produces<ApiResponse<LinkGoogleResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
