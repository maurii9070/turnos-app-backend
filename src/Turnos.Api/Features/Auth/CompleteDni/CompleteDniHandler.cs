using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Auth.CompleteDni;

public sealed class CompleteDniHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<CompleteDniResponse>> HandleAsync(
        CompleteDniRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<CompleteDniResponse>.Fail("Usuario no autenticado.");
        }

        var dni = request.Dni.Trim();

        var user = await dbContext.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return ApiResponse<CompleteDniResponse>.Fail("Usuario no encontrado.");
        }

        var dniExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Dni == dni && u.Id != userId, cancellationToken);

        if (dniExists)
        {
            return ApiResponse<CompleteDniResponse>.Fail(
                "Ya existe una cuenta con ese DNI. Iniciá sesión con DNI y contraseña y vinculá Google desde tu perfil.");
        }

        user.Dni = dni;
        user.RequiresDni = false;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CompleteDniResponse(user.Id, user.Dni);
        return ApiResponse<CompleteDniResponse>.Ok(response, "DNI guardado correctamente.");
    }
}
