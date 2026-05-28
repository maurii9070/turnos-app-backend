using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Specialties.DeleteSpecialty;

public class DeleteSpecialtyHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<object>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var specialty = await dbContext.Specialties
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, cancellationToken);

        if (specialty is null)
        {
            return ApiResponse<object>.Fail("Especialidad no encontrada.");
        }

        specialty.IsActive = false;
        specialty.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Especialidad eliminada correctamente.");
    }
}
