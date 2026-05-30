namespace Turnos.Api.Features.Doctors.Availabilities.ListAvailabilities;

public record ListAvailabilitiesResponse(Guid Id, string Date, TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable);
