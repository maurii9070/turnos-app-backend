using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Specialties.GetSpecialties;

public class GetSpecialtiesHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<GetSpecialtiesResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        var specialties = await dbContext.Specialties
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new GetSpecialtiesResponse(s.Id, s.Name, s.Description))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<GetSpecialtiesResponse>>.Ok(specialties);
    }
}
