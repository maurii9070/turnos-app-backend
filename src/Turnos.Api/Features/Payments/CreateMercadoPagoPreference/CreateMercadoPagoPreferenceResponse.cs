namespace Turnos.Api.Features.Payments.CreateMercadoPagoPreference;

public record CreateMercadoPagoPreferenceResponse(
    Guid PaymentId,
    Guid AppointmentId,
    decimal Amount,
    string Status,
    string PreferenceId,
    string InitPoint
);