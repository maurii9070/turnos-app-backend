using FluentValidation;

namespace Turnos.Api.Features.Doctors.Schedules.SetSchedules;

public class SetSchedulesValidator : AbstractValidator<SetSchedulesRequest>
{
    public SetSchedulesValidator()
    {
        RuleForEach(x => x.Schedules).ChildRules(item =>
        {
            item.RuleFor(x => x.DayOfWeek)
                .NotEmpty().WithMessage("El día es obligatorio.")
                .Must(d => Enum.TryParse<DayOfWeek>(d, out _))
                .WithMessage("Día de semana inválido. Use: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday.");

            item.RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("La hora de inicio es obligatoria.");

            item.RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("La hora de fin es obligatoria.")
                .GreaterThan(x => x.StartTime)
                .WithMessage("La hora de fin debe ser posterior a la de inicio.");
        });
    }
}
