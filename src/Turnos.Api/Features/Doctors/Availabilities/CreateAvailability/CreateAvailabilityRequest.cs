namespace Turnos.Api.Features.Doctors.Availabilities.CreateAvailability;

public record CreateAvailabilityRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable
);
