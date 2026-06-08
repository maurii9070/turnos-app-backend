namespace Turnos.Api.Features.AppointmentFiles.UploadAppointmentFile;

public record UploadAppointmentFileResponse(
    Guid Id,
    Guid AppointmentId,
    string FileName,
    string FileType,
    string FilePathOrUrl,
    string Category,
    DateTime UploadedAt
);
