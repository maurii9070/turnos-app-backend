using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.CreatePayment;

public record CreatePaymentRequest(
    PaymentMethod Method,
    string? ReceiptUrl
);
