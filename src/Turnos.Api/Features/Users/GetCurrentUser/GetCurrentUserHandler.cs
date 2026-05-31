using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Users.GetCurrentUser;

public class GetCurrentUserHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<GetCurrentUserResponse>> HandleAsync(
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<GetCurrentUserResponse>.Fail("Usuario no autenticado.");
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return ApiResponse<GetCurrentUserResponse>.Fail("Usuario no encontrado.");
        }

        var response = new GetCurrentUserResponse(
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

        return ApiResponse<GetCurrentUserResponse>.Ok(response);
    }
}
