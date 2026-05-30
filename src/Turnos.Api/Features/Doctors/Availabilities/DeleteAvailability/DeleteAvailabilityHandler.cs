using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.Availabilities.DeleteAvailability;

public class DeleteAvailabilityHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<object>> HandleAsync(
        Guid doctorId,
        Guid availabilityId,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var availability = await dbContext.DoctorAvailabilities
            .AsTracking()
            .FirstOrDefaultAsync(a => a.Id == availabilityId && a.DoctorId == doctorId && a.IsActive, cancellationToken);

        if (availability is null)
        {
            return ApiResponse<object>.Fail("Disponibilidad no encontrada.");
        }

        if (!await DoctorAuthorization.CanManageDoctorAsync(doctorId, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<object>.Fail("No autorizado.");
        }

        availability.IsActive = false;
        availability.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { }, "Disponibilidad eliminada correctamente.");
    }
}
