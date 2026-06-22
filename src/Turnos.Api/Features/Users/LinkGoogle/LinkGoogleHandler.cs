using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Users.LinkGoogle;

public sealed class LinkGoogleHandler(TurnosDbContext dbContext, ISupabaseAuthClient supabaseAuthClient)
{
    public async Task<ApiResponse<LinkGoogleResponse>> HandleAsync(
        LinkGoogleRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<LinkGoogleResponse>.Fail("Usuario no autenticado.");
        }

        var supabaseUser = await supabaseAuthClient.GetUserInfoAsync(request.SupabaseToken, cancellationToken);

        if (supabaseUser is null)
        {
            return ApiResponse<LinkGoogleResponse>.Fail("Token de Google inválido.");
        }

        if (!supabaseUser.EmailVerified)
        {
            return ApiResponse<LinkGoogleResponse>.Fail("El email de Google no está verificado.");
        }

        var googleIdInUse = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.GoogleId == supabaseUser.Id && u.Id != userId, cancellationToken);

        if (googleIdInUse)
        {
            return ApiResponse<LinkGoogleResponse>.Fail("Esta cuenta de Google ya está vinculada a otro usuario.");
        }

        var user = await dbContext.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return ApiResponse<LinkGoogleResponse>.Fail("Usuario no encontrado.");
        }

        if (!string.IsNullOrEmpty(user.GoogleId))
        {
            return ApiResponse<LinkGoogleResponse>.Fail("Tu cuenta ya tiene vinculada una cuenta de Google.");
        }

        user.GoogleId = supabaseUser.Id;
        user.Email = supabaseUser.Email ?? user.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new LinkGoogleResponse(user.Id, user.GoogleId);
        return ApiResponse<LinkGoogleResponse>.Ok(response, "Cuenta de Google vinculada correctamente.");
    }
}
