namespace Turnos.Api.Features.Doctors.Schedules.SetSchedules;

public record SetSchedulesResponse(Guid Id, string DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
