using FluentValidation;

namespace Turnos.Api.Features.Users.UpdateProfile;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => x.Email is not null).WithMessage("El email no tiene un formato válido.")
            .MaximumLength(256).When(x => x.Email is not null).WithMessage("El email no puede superar los 256 caracteres.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).When(x => x.Phone is not null).WithMessage("El teléfono no puede superar los 30 caracteres.");

        RuleFor(x => x.DateOfBirth)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.Today))
                .When(x => x.DateOfBirth is not null)
                .WithMessage("La fecha de nacimiento no puede ser futura.");
    }
}
