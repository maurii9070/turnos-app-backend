using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.Availabilities.UpdateAvailability;

public class UpdateAvailabilityHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<UpdateAvailabilityResponse>> HandleAsync(
        Guid doctorId,
        Guid availabilityId,
        UpdateAvailabilityRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var availability = await dbContext.DoctorAvailabilities
            .AsTracking()
            .FirstOrDefaultAsync(a => a.Id == availabilityId && a.DoctorId == doctorId && a.IsActive, cancellationToken);

        if (availability is null)
        {
            return ApiResponse<UpdateAvailabilityResponse>.Fail("Disponibilidad no encontrada.");
        }

        if (!await DoctorAuthorization.CanManageDoctorAsync(doctorId, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<UpdateAvailabilityResponse>.Fail("No autorizado.");
        }

        var duplicateDate = await dbContext.DoctorAvailabilities
            .AsNoTracking()
            .AnyAsync(a => a.DoctorId == doctorId && a.Date == request.Date && a.Id != availabilityId, cancellationToken);

        if (duplicateDate)
        {
            return ApiResponse<UpdateAvailabilityResponse>.Fail("Ya existe otra disponibilidad para esa misma fecha.");
        }

        availability.Date = request.Date;
        availability.StartTime = request.StartTime;
        availability.EndTime = request.EndTime;
        availability.IsAvailable = request.IsAvailable;
        availability.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateAvailabilityResponse(
            availability.Id,
            availability.Date.ToString("yyyy-MM-dd"),
            availability.StartTime,
            availability.EndTime,
            availability.IsAvailable);

        return ApiResponse<UpdateAvailabilityResponse>.Ok(response, "Disponibilidad actualizada correctamente.");
    }
}
