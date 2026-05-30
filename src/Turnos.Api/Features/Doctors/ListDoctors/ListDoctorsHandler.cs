using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.ListDoctors;

public class ListDoctorsHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<ListDoctorsResponse>>> HandleAsync(
        Guid? specialtyId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.IsActive && d.User.IsActive);

        if (specialtyId.HasValue)
        {
            query = query.Where(d => d.SpecialtyId == specialtyId.Value);
        }

        var doctors = await query
            .OrderBy(d => d.User.LastName)
            .ThenBy(d => d.User.FirstName)
            .Select(d => new ListDoctorsResponse(
                d.Id,
                d.UserId,
                d.User.Dni,
                d.User.FirstName,
                d.User.LastName,
                d.SpecialtyId,
                d.Specialty.Name,
                d.LicenseNumber,
                d.ConsultationPrice))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ListDoctorsResponse>>.Ok(doctors);
    }
}
