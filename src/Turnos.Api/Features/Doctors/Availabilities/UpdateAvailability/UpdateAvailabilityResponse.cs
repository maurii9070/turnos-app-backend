namespace Turnos.Api.Features.Doctors.Availabilities.UpdateAvailability;

public record UpdateAvailabilityResponse(
    Guid Id,
    string Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable
);
