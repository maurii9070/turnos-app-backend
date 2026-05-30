using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;

namespace Turnos.Api.Features.Doctors.Availabilities.CreateAvailability;

public class CreateAvailabilityHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<CreateAvailabilityResponse>> HandleAsync(
        Guid doctorId,
        CreateAvailabilityRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        if (!await DoctorAuthorization.CanManageDoctorAsync(doctorId, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<CreateAvailabilityResponse>.Fail("No autorizado.");
        }

        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == doctorId && d.IsActive, cancellationToken);

        if (!doctorExists)
        {
            return ApiResponse<CreateAvailabilityResponse>.Fail("Doctor no encontrado.");
        }

        var duplicateDate = await dbContext.DoctorAvailabilities
            .AsNoTracking()
            .AnyAsync(a => a.DoctorId == doctorId && a.Date == request.Date, cancellationToken);

        if (duplicateDate)
        {
            return ApiResponse<CreateAvailabilityResponse>.Fail("Ya existe una disponibilidad para esa fecha.");
        }

        var availability = new DoctorAvailability
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = request.IsAvailable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.DoctorAvailabilities.Add(availability);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateAvailabilityResponse(
            availability.Id,
            availability.Date.ToString("yyyy-MM-dd"),
            availability.StartTime,
            availability.EndTime,
            availability.IsAvailable);

        return ApiResponse<CreateAvailabilityResponse>.Ok(response, "Disponibilidad creada correctamente.");
    }
}
