using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Patients.ListPatients;

public class ListPatientsHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<ListPatientsResponse>>> HandleAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Patient);

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        var patients = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new ListPatientsResponse(
                u.Patient != null ? u.Patient.Id : Guid.Empty,
                u.Id,
                u.Dni,
                u.FirstName,
                u.LastName,
                u.Phone,
                u.Patient != null ? u.Patient.DateOfBirth : null,
                u.IsActive,
                u.Patient != null
                    ? dbContext.Appointments.Count(a => a.PatientId == u.Patient.Id)
                    : 0,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ListPatientsResponse>>.Ok(patients);
    }
}
