using FluentValidation;

namespace Turnos.Api.Features.Auth.CompleteDni;

public class CompleteDniValidator : AbstractValidator<CompleteDniRequest>
{
    public CompleteDniValidator()
    {
        RuleFor(x => x.Dni)
            .NotEmpty()
            .WithMessage("El DNI es obligatorio.")
            .MaximumLength(20)
            .WithMessage("El DNI no puede tener más de 20 caracteres.");
    }
}
