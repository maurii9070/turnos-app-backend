using FluentValidation;

namespace Turnos.Api.Features.Doctors.CreateDoctor;

public class CreateDoctorValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorValidator()
    {
        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es obligatorio.")
            .MaximumLength(20).WithMessage("El DNI no puede superar los 20 caracteres.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.SpecialtyId)
            .NotEmpty().WithMessage("La especialidad es obligatoria.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("El número de licencia es obligatorio.")
            .MaximumLength(50).WithMessage("El número de licencia no puede superar los 50 caracteres.");

        RuleFor(x => x.ConsultationPrice)
            .GreaterThan(0).WithMessage("El precio de consulta debe ser mayor a cero.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => x.Email is not null).WithMessage("El email no tiene un formato válido.")
            .MaximumLength(256).When(x => x.Email is not null).WithMessage("El email no puede superar los 256 caracteres.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).When(x => x.Phone is not null).WithMessage("El teléfono no puede superar los 30 caracteres.");
    }
}
