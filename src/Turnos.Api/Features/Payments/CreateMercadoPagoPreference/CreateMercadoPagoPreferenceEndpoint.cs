using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Payments.CreateMercadoPagoPreference;

public class CreateMercadoPagoPreferenceEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/appointments/{id:guid}/mercadopago/preference", async (
            Guid id,
            CreateMercadoPagoPreferenceRequest? request,
            ClaimsPrincipal user,
            CreateMercadoPagoPreferenceHandler handler,
            IValidator<CreateMercadoPagoPreferenceRequest> validator,
            CancellationToken ct) =>
        {
            request ??= new CreateMercadoPagoPreferenceRequest(null, null, null);

            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(id, request, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "Turno no encontrado." => Results.NotFound(result),
                    "No tiene permiso para pagar este turno." => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    "Usuario no autenticado." => Results.Json(result, statusCode: StatusCodes.Status401Unauthorized),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("CreateMercadoPagoPreference")
        .RequireAuthorization()
        .Produces<ApiResponse<CreateMercadoPagoPreferenceResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}