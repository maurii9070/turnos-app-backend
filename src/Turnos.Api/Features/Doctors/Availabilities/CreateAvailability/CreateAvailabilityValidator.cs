using FluentValidation;

namespace Turnos.Api.Features.Doctors.Availabilities.CreateAvailability;

public class CreateAvailabilityValidator : AbstractValidator<CreateAvailabilityRequest>
{
    public CreateAvailabilityValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("La hora de inicio es obligatoria.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("La hora de fin es obligatoria.")
            .GreaterThan(x => x.StartTime)
            .WithMessage("La hora de fin debe ser posterior a la de inicio.");
    }
}
