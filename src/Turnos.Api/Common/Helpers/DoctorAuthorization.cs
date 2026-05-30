using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Common.Helpers;

public static class DoctorAuthorization
{
    public static async Task<bool> CanManageDoctorAsync(
        Guid doctorId,
        ClaimsPrincipal user,
        TurnosDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (user.IsInRole(nameof(UserRole.SuperAdmin)))
            return true;

        if (!user.IsInRole(nameof(UserRole.Doctor)))
            return false;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return false;

        return await dbContext.Doctors.AnyAsync(
            d => d.Id == doctorId && d.UserId == userId, cancellationToken);
    }
}
