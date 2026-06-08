namespace Turnos.Api.Features.Appointments.ListDoctorAppointments;

public record ListDoctorAppointmentsResponse(
    Guid Id,
    Guid DoctorId,
    string DoctorFirstName,
    string DoctorLastName,
    string? SpecialtyName,
    Guid PatientId,
    string PatientFirstName,
    string PatientLastName,
    string Date,
    string StartTime,
    string Status,
    string? Notes
);