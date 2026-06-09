namespace Turnos.Api.Features.Appointments.ListMyAppointments;

public record ListMyAppointmentsResponse(
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
    string? Notes,
    string? PaymentMethod,
    Guid? PaymentId
);
