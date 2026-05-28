using FluentValidation;

namespace Turnos.Api.Features.Specialties.UpdateSpecialty;

public class UpdateSpecialtyValidator : AbstractValidator<UpdateSpecialtyRequest>
{
    public UpdateSpecialtyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
