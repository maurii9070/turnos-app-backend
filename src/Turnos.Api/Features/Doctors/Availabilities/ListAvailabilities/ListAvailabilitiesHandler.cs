using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.Availabilities.ListAvailabilities;

public class ListAvailabilitiesHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<ListAvailabilitiesResponse>>> HandleAsync(
        Guid doctorId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == doctorId && d.IsActive, cancellationToken);

        if (!doctorExists)
        {
            return ApiResponse<List<ListAvailabilitiesResponse>>.Fail("Doctor no encontrado.");
        }

        var query = dbContext.DoctorAvailabilities
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.IsActive);

        if (from.HasValue)
        {
            query = query.Where(a => a.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.Date <= to.Value);
        }

        var availabilities = await query
            .OrderBy(a => a.Date)
            .ThenBy(a => a.StartTime)
            .Select(a => new ListAvailabilitiesResponse(
                a.Id,
                a.Date.ToString("yyyy-MM-dd"),
                a.StartTime,
                a.EndTime,
                a.IsAvailable))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ListAvailabilitiesResponse>>.Ok(availabilities);
    }
}
