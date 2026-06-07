namespace Turnos.Api.Features.AppointmentFiles.UploadAppointmentFile;

public record UploadAppointmentFileRequest(
    string FileName,
    string FileType,
    string FilePathOrUrl
);
