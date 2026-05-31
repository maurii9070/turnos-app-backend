using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Users.ChangePassword;

public class ChangePasswordHandler(TurnosDbContext dbContext, IPasswordHasher passwordHasher)
{
    public async Task<ApiResponse<ChangePasswordResponse>> HandleAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<ChangePasswordResponse>.Fail("Usuario no autenticado.");
        }

        var user = await dbContext.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return ApiResponse<ChangePasswordResponse>.Fail("Usuario no encontrado.");
        }

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse<ChangePasswordResponse>.Fail("La contraseña actual es incorrecta.");
        }

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ChangePasswordResponse>.Ok(
            new ChangePasswordResponse(user.Id),
            "Contraseña actualizada correctamente.");
    }
}
