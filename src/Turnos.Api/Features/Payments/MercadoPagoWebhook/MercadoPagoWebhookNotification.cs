using System.Text.Json.Serialization;

namespace Turnos.Api.Features.Payments.MercadoPagoWebhook;

public record MercadoPagoWebhookNotification(
    [property: JsonPropertyName("id")] long? Id,
    [property: JsonPropertyName("live_mode")] bool LiveMode,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("date_created")] string? DateCreated,
    [property: JsonPropertyName("user_id")] long? UserId,
    [property: JsonPropertyName("api_version")] string? ApiVersion,
    [property: JsonPropertyName("action")] string? Action,
    [property: JsonPropertyName("data")] MercadoPagoWebhookData? Data
);

public record MercadoPagoWebhookData(
    [property: JsonPropertyName("id")] string? Id
);