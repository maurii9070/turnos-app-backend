using FluentValidation;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.CreatePayment;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.Method)
            .Must(m => m != PaymentMethod.MercadoPago)
            .WithMessage("MercadoPago se gestiona en otro endpoint. Use 'Cash' o 'Transfer'.");

        RuleFor(x => x.ReceiptUrl)
            .MaximumLength(500).WithMessage("La URL del comprobante no puede superar los 500 caracteres.")
            .Must(BeAValidUri).WithMessage("La URL del comprobante no es válida.")
            .When(x => !string.IsNullOrEmpty(x.ReceiptUrl));
    }

    private static bool BeAValidUri(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
