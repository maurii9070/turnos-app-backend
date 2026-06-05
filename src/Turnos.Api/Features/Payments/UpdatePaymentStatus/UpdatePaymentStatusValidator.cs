using FluentValidation;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.UpdatePaymentStatus;

public class UpdatePaymentStatusValidator : AbstractValidator<UpdatePaymentStatusRequest>
{
    public UpdatePaymentStatusValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s == PaymentStatus.Approved || s == PaymentStatus.Rejected)
            .WithMessage("El estado solo puede ser 'Approved' o 'Rejected'.");
    }
}
