using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Specialties.GetSpecialtyById;

public class GetSpecialtyByIdHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<GetSpecialtyByIdResponse>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var specialty = await dbContext.Specialties
            .AsNoTracking()
            .Where(s => s.Id == id && s.IsActive)
            .Select(s => new GetSpecialtyByIdResponse(s.Id, s.Name, s.Description))
            .FirstOrDefaultAsync(cancellationToken);

        if (specialty is null)
        {
            return ApiResponse<GetSpecialtyByIdResponse>.Fail("Especialidad no encontrada.");
        }

        return ApiResponse<GetSpecialtyByIdResponse>.Ok(specialty);
    }
}
