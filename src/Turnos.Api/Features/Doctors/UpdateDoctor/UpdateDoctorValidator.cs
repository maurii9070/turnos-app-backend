using FluentValidation;

namespace Turnos.Api.Features.Doctors.UpdateDoctor;

public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorValidator()
    {
        RuleFor(x => x.SpecialtyId)
            .NotEmpty().WithMessage("La especialidad es obligatoria.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("El número de licencia es obligatorio.")
            .MaximumLength(50).WithMessage("El número de licencia no puede superar los 50 caracteres.");

        RuleFor(x => x.ConsultationPrice)
            .GreaterThan(0).WithMessage("El precio de consulta debe ser mayor a cero.");
    }
}
