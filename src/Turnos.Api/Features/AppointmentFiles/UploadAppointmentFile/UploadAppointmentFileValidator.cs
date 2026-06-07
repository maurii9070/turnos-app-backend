using FluentValidation;

namespace Turnos.Api.Features.AppointmentFiles.UploadAppointmentFile;

public class UploadAppointmentFileValidator : AbstractValidator<UploadAppointmentFileRequest>
{
    public UploadAppointmentFileValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("El nombre del archivo es obligatorio.")
            .MaximumLength(255).WithMessage("El nombre del archivo no puede superar los 255 caracteres.");

        RuleFor(x => x.FileType)
            .NotEmpty().WithMessage("El tipo de archivo es obligatorio.")
            .MaximumLength(50).WithMessage("El tipo de archivo no puede superar los 50 caracteres.");

        RuleFor(x => x.FilePathOrUrl)
            .NotEmpty().WithMessage("La URL del archivo es obligatoria.")
            .MaximumLength(500).WithMessage("La URL del archivo no puede superar los 500 caracteres.")
            .Must(BeAValidUri).WithMessage("La URL del archivo no es válida.");
    }

    private static bool BeAValidUri(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
