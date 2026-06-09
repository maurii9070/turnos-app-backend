using Turnos.Api.Common.Contracts;

namespace Turnos.Api.Features.Payments.MercadoPagoWebhook;

public class MercadoPagoWebhookEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/payments/mercadopago/webhook", async (
            MercadoPagoWebhookNotification notification,
            MercadoPagoWebhookHandler handler,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var xSignature = httpContext.Request.Headers["x-signature"].FirstOrDefault() ?? string.Empty;
            var xRequestId = httpContext.Request.Headers["x-request-id"].FirstOrDefault() ?? string.Empty;
            var dataId = httpContext.Request.Query["data.id"].FirstOrDefault() ?? notification.Data?.Id ?? string.Empty;

            return await handler.HandleWithValidationAsync(notification, xSignature, xRequestId, dataId, ct);
        })
        .WithName("MercadoPagoWebhook")
        .AllowAnonymous()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status401Unauthorized);
    }
}