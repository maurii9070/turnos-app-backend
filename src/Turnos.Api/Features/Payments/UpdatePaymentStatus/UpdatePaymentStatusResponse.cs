namespace Turnos.Api.Features.Payments.UpdatePaymentStatus;

public record UpdatePaymentStatusResponse(
    Guid Id,
    Guid AppointmentId,
    decimal Amount,
    string Method,
    string Status,
    string? ReceiptUrl,
    DateTime? PaidAt,
    DateTime UpdatedAt
);
