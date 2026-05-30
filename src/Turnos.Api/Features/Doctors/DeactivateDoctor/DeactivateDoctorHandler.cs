using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.DeactivateDoctor;

public class DeactivateDoctorHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<object>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Doctors
            .AsTracking()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive && d.User.IsActive, cancellationToken);

        if (doctor is null)
        {
            return ApiResponse<object>.Fail("Doctor no encontrado.");
        }

        doctor.IsActive = false;
        doctor.UpdatedAt = DateTime.UtcNow;
        doctor.User.IsActive = false;
        doctor.User.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Doctor desactivado correctamente.");
    }
}
