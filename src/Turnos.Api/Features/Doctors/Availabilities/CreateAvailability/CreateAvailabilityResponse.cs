namespace Turnos.Api.Features.Doctors.Availabilities.CreateAvailability;

public record CreateAvailabilityResponse(
    Guid Id,
    string Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable
);
