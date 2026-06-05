using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Payments.UpdatePaymentStatus;

public class UpdatePaymentStatusEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/payments/{id:guid}", async (
            Guid id,
            UpdatePaymentStatusRequest request,
            ClaimsPrincipal user,
            UpdatePaymentStatusHandler handler,
            IValidator<UpdatePaymentStatusRequest> validator,
            CancellationToken ct) =>
        {
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
                    "Pago no encontrado." => Results.NotFound(result),
                    "No tiene permiso para modificar este pago." => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("UpdatePaymentStatus")
        .RequireAuthorization()
        .Produces<ApiResponse<UpdatePaymentStatusResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
