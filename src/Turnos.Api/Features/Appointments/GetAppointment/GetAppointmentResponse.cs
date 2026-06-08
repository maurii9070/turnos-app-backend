namespace Turnos.Api.Features.Appointments.GetAppointment;

public record GetAppointmentResponse(
    Guid Id,
    Guid PatientId,
    string PatientFirstName,
    string PatientLastName,
    string PatientDni,
    Guid DoctorId,
    string DoctorFirstName,
    string DoctorLastName,
    string? DoctorEmail,
    string DoctorLicenseNumber,
    string? SpecialtyName,
    decimal ConsultationPrice,
    string Date,
    string StartTime,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<AppointmentFileResponse> Files
);

public record AppointmentFileResponse(
    Guid Id,
    Guid AppointmentId,
    string FilePathOrUrl,
    string FileName,
    string FileType,
    DateTime UploadedAt
);
