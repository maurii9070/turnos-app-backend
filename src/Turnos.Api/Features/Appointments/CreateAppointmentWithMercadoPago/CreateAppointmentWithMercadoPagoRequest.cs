namespace Turnos.Api.Features.Appointments.CreateAppointmentWithMercadoPago;

public record CreateAppointmentWithMercadoPagoRequest(
    Guid DoctorId,
    DateOnly Date,
    TimeOnly StartTime,
    string? Notes,
    string? PayerEmail,
    string? PayerFirstName,
    string? PayerLastName
);