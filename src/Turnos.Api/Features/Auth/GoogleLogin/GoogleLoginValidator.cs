using FluentValidation;

namespace Turnos.Api.Features.Auth.GoogleLogin;

public class GoogleLoginValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.SupabaseToken)
            .NotEmpty()
            .WithMessage("El token de Supabase es obligatorio.");
    }
}
