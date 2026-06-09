namespace Turnos.Api.Features.Payments.CreateMercadoPagoPreference;

public record CreateMercadoPagoPreferenceRequest(
    string? PayerEmail,
    string? PayerFirstName,
    string? PayerLastName
);