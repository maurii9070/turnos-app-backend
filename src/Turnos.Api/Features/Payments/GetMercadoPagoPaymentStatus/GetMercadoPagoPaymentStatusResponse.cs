namespace Turnos.Api.Features.Payments.GetMercadoPagoPaymentStatus;

public record GetMercadoPagoPaymentStatusResponse(
    Guid PaymentId,
    Guid AppointmentId,
    decimal Amount,
    string Status,
    string? PreferenceId,
    string? MercadoPagoPaymentId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);