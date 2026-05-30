using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.Schedules.GetSchedules;

public class GetSchedulesHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<GetSchedulesResponse>>> HandleAsync(
        Guid doctorId,
        CancellationToken cancellationToken)
    {
        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == doctorId && d.IsActive, cancellationToken);

        if (!doctorExists)
        {
            return ApiResponse<List<GetSchedulesResponse>>.Fail("Doctor no encontrado.");
        }

        var schedules = await dbContext.Schedules
            .AsNoTracking()
            .Where(s => s.DoctorId == doctorId && s.IsActive)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new GetSchedulesResponse(
                s.Id,
                s.DayOfWeek.ToString(),
                s.StartTime,
                s.EndTime))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<GetSchedulesResponse>>.Ok(schedules);
    }
}
