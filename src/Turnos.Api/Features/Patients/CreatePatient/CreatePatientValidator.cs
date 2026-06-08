using FluentValidation;

namespace Turnos.Api.Features.Patients.CreatePatient;

public class CreatePatientValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientValidator()
    {
        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es obligatorio.")
            .Must(BeOnlyDigits).WithMessage("El DNI debe contener solo números.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede superar los 20 caracteres.");
    }

    private static bool BeOnlyDigits(string dni)
    {
        return !string.IsNullOrWhiteSpace(dni) && dni.All(char.IsDigit);
    }
}
