namespace Turnos.Api.Features.Payments.CreatePayment;

public record CreatePaymentResponse(
    Guid Id,
    Guid AppointmentId,
    decimal Amount,
    string Method,
    string Status,
    string? ReceiptUrl,
    DateTime CreatedAt
);
