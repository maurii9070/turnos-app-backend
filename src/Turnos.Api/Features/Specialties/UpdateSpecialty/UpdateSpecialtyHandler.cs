using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Specialties.UpdateSpecialty;

public class UpdateSpecialtyHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<UpdateSpecialtyResponse>> HandleAsync(
        Guid id,
        UpdateSpecialtyRequest request,
        CancellationToken cancellationToken)
    {
        var specialty = await dbContext.Specialties
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, cancellationToken);

        if (specialty is null)
        {
            return ApiResponse<UpdateSpecialtyResponse>.Fail("Especialidad no encontrada.");
        }

        var duplicateName = await dbContext.Specialties
            .AsNoTracking()
            .AnyAsync(s => s.Name.ToLower() == request.Name.Trim().ToLower()
                        && s.Id != id, cancellationToken);

        if (duplicateName)
        {
            return ApiResponse<UpdateSpecialtyResponse>.Fail("Ya existe una especialidad con ese nombre.");
        }

        specialty.Name = request.Name.Trim();
        specialty.Description = request.Description?.Trim();
        specialty.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateSpecialtyResponse(specialty.Id, specialty.Name, specialty.Description);
        return ApiResponse<UpdateSpecialtyResponse>.Ok(response, "Especialidad actualizada correctamente.");
    }
}
