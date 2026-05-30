using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;

namespace Turnos.Api.Features.Doctors.Schedules.SetSchedules;

public class SetSchedulesHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<SetSchedulesResponse>>> HandleAsync(
        Guid doctorId,
        SetSchedulesRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        if (!await DoctorAuthorization.CanManageDoctorAsync(doctorId, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<List<SetSchedulesResponse>>.Fail("No autorizado.");
        }

        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == doctorId && d.IsActive, cancellationToken);

        if (!doctorExists)
        {
            return ApiResponse<List<SetSchedulesResponse>>.Fail("Doctor no encontrado.");
        }

        var existingSchedules = await dbContext.Schedules
            .AsTracking()
            .Where(s => s.DoctorId == doctorId && s.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var schedule in existingSchedules)
        {
            schedule.IsActive = false;
            schedule.UpdatedAt = DateTime.UtcNow;
        }

        var newSchedules = request.Schedules.Select(item => new Schedule
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            DayOfWeek = Enum.Parse<DayOfWeek>(item.DayOfWeek),
            StartTime = item.StartTime,
            EndTime = item.EndTime,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        dbContext.Schedules.AddRange(newSchedules);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = newSchedules.Select(s => new SetSchedulesResponse(
            s.Id,
            s.DayOfWeek.ToString(),
            s.StartTime,
            s.EndTime)).ToList();

        return ApiResponse<List<SetSchedulesResponse>>.Ok(response, "Horarios actualizados correctamente.");
    }
}
