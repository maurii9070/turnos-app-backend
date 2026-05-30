namespace Turnos.Api.Features.Appointments.CreateAppointment;

public record CreateAppointmentRequest(
    Guid DoctorId,
    DateOnly Date,
    TimeOnly StartTime,
    string? Notes
);
