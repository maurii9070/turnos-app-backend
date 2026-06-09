namespace Turnos.Api.Features.Payments.SyncMercadoPagoPayment;

public record SyncMercadoPagoPaymentResponse(
    Guid PaymentId,
    Guid AppointmentId,
    string PreviousStatus,
    string CurrentStatus,
    bool AppointmentConfirmed
);