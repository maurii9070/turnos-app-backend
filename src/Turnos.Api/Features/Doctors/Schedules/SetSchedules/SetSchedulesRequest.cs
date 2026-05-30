namespace Turnos.Api.Features.Doctors.Schedules.SetSchedules;

public record SetScheduleItem(string DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

public record SetSchedulesRequest(List<SetScheduleItem> Schedules);
