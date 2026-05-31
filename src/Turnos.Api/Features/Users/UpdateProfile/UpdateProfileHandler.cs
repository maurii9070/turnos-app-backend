using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Users.UpdateProfile;

public class UpdateProfileHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<UpdateProfileResponse>> HandleAsync(
        UpdateProfileRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<UpdateProfileResponse>.Fail("Usuario no autenticado.");
        }

        var user = await dbContext.Users
            .AsTracking()
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UpdateProfileResponse>.Fail("Usuario no encontrado.");
        }

        if (request.Email is not null && request.Email != user.Email)
        {
            var emailExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == request.Email && u.Id != userId, cancellationToken);

            if (emailExists)
            {
                return ApiResponse<UpdateProfileResponse>.Fail("El email ya está en uso por otro usuario.");
            }
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email?.Trim();
        user.Phone = request.Phone?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        if (user.Patient is not null && request.DateOfBirth is not null)
        {
            user.Patient.DateOfBirth = request.DateOfBirth.Value;
            user.Patient.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateProfileResponse(
            user.Id,
            user.Dni,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Role.ToString(),
            user.MustChangePassword,
            user.Patient?.Id,
            user.Patient?.DateOfBirth,
            user.Doctor?.Id);

        return ApiResponse<UpdateProfileResponse>.Ok(response, "Perfil actualizado correctamente.");
    }
}
