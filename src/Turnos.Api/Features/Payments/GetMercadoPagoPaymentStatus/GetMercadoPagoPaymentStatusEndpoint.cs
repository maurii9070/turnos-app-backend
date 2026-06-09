using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Payments.GetMercadoPagoPaymentStatus;

public class GetMercadoPagoPaymentStatusEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("api/payments/mercadopago/{paymentId:guid}/status", async (
            Guid paymentId,
            ClaimsPrincipal user,
            GetMercadoPagoPaymentStatusHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(paymentId, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "Pago no encontrado." => Results.NotFound(result),
                    "Usuario no autenticado." => Results.Json(result, statusCode: StatusCodes.Status401Unauthorized),
                    _ => Results.BadRequest(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("GetMercadoPagoPaymentStatus")
        .RequireAuthorization()
        .Produces<ApiResponse<GetMercadoPagoPaymentStatusResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}