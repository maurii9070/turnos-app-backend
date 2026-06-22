using FluentValidation;

namespace Turnos.Api.Features.Users.LinkGoogle;

public class LinkGoogleValidator : AbstractValidator<LinkGoogleRequest>
{
    public LinkGoogleValidator()
    {
        RuleFor(x => x.SupabaseToken)
            .NotEmpty()
            .WithMessage("El token de Supabase es obligatorio.");
    }
}
