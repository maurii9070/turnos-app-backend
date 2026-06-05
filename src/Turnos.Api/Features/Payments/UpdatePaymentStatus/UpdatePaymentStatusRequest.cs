using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.UpdatePaymentStatus;

public record UpdatePaymentStatusRequest(
    PaymentStatus Status
);
