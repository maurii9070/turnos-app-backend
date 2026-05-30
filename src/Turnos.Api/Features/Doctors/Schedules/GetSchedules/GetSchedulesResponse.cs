namespace Turnos.Api.Features.Doctors.Schedules.GetSchedules;

public record GetSchedulesResponse(Guid Id, string DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
