namespace Turnos.Api.Features.Appointments.CreateAppointmentWithMercadoPago;

public record CreateAppointmentWithMercadoPagoResponse(
    Guid AppointmentId,
    Guid PaymentId,
    decimal Amount,
    string InitPoint,
    string PreferenceId
);