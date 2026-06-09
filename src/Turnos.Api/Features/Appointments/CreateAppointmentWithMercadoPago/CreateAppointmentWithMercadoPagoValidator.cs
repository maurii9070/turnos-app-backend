using FluentValidation;

namespace Turnos.Api.Features.Appointments.CreateAppointmentWithMercadoPago;

public class CreateAppointmentWithMercadoPagoValidator : AbstractValidator<CreateAppointmentWithMercadoPagoRequest>
{
    public CreateAppointmentWithMercadoPagoValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("El doctor es obligatorio.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha debe ser hoy o posterior.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("La hora de inicio es obligatoria.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Las notas no pueden superar los 2000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Notes));

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