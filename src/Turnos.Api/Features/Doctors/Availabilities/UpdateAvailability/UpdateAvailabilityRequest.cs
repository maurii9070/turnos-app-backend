namespace Turnos.Api.Features.Doctors.Availabilities.UpdateAvailability;

public record UpdateAvailabilityRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable
);
