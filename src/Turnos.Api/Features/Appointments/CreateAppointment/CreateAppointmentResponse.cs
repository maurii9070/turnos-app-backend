namespace Turnos.Api.Features.Appointments.CreateAppointment;

public record CreateAppointmentResponse(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    string Date,
    string StartTime,
    string Status,
    string? Notes
);
