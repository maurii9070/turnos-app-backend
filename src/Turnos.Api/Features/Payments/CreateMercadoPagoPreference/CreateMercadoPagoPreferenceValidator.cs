using FluentValidation;

namespace Turnos.Api.Features.Payments.CreateMercadoPagoPreference;

public class CreateMercadoPagoPreferenceValidator : AbstractValidator<CreateMercadoPagoPreferenceRequest>
{
    public CreateMercadoPagoPreferenceValidator()
    {
        When(x => x.PayerEmail is not null, () =>
        {
            RuleFor(x => x.PayerEmail)
                .EmailAddress().WithMessage("El email del pagador no es válido.")
                .MaximumLength(256);
        });

        When(x => x.PayerFirstName is not null, () =>
        {
            RuleFor(x => x.PayerFirstName)
                .MaximumLength(100);
        });

        When(x => x.PayerLastName is not null, () =>
        {
            RuleFor(x => x.PayerLastName)
                .MaximumLength(100);
        });
    }
}