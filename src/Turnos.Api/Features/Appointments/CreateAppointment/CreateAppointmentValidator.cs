using FluentValidation;

namespace Turnos.Api.Features.Appointments.CreateAppointment;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentValidator()
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
    }
}
