using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.AppointmentFiles.UploadAppointmentFile;

public record UploadAppointmentFileRequest(
    string FileName,
    string FileType,
    string FilePathOrUrl,
    AppointmentFileCategory Category
);
