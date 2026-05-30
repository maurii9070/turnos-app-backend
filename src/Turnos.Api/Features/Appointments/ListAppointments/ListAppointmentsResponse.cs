namespace Turnos.Api.Features.Appointments.ListAppointments;

public record ListAppointmentsResponse(
    Guid Id,
    Guid PatientId,
    string PatientFirstName,
    string PatientLastName,
    string PatientDni,
    Guid DoctorId,
    string DoctorFirstName,
    string DoctorLastName,
    string? SpecialtyName,
    string Date,
    string StartTime,
    string Status,
    string? Notes,
    DateTime CreatedAt
);
