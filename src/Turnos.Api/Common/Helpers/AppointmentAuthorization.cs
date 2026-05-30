using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Common.Helpers;

public static class AppointmentAuthorization
{
    public static async Task<bool> CanAccessAppointmentAsync(
        Guid appointmentId,
        ClaimsPrincipal user,
        TurnosDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return false;

        if (user.IsInRole(nameof(UserRole.SuperAdmin)))
            return true;

        return await dbContext.Appointments.AnyAsync(
            a => a.Id == appointmentId &&
                 (a.Patient.UserId == userId || a.Doctor.UserId == userId),
            cancellationToken);
    }

    public static async Task<bool> IsDoctorOfAppointmentAsync(
        Guid appointmentId,
        ClaimsPrincipal user,
        TurnosDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return false;

        if (user.IsInRole(nameof(UserRole.SuperAdmin)))
            return true;

        return await dbContext.Appointments.AnyAsync(
            a => a.Id == appointmentId && a.Doctor.UserId == userId,
            cancellationToken);
    }
}
