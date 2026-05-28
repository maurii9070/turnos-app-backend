using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenHandler(TurnosDbContext dbContext, ITokenService tokenService)
{
    public async Task<(ApiResponse<RefreshTokenResponse> Response, string? NewRefreshToken)> HandleAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken is null)
        {
            return (ApiResponse<RefreshTokenResponse>.Fail("Refresh token inválido."), null);
        }

        if (refreshToken.IsUsed)
        {
            return (ApiResponse<RefreshTokenResponse>.Fail("Refresh token ya fue utilizado."), null);
        }

        if (refreshToken.RevokedAt is not null)
        {
            return (ApiResponse<RefreshTokenResponse>.Fail("Refresh token revocado."), null);
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return (ApiResponse<RefreshTokenResponse>.Fail("Refresh token expirado."), null);
        }

        refreshToken.IsUsed = true;
        refreshToken.RevokedAt = DateTime.UtcNow;

        var user = refreshToken.User;
        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();

        var newRefreshToken = new Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false
        };

        dbContext.RefreshTokens.Update(refreshToken);
        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(newAccessToken, user.Role.ToString());
        return (ApiResponse<RefreshTokenResponse>.Ok(response, "Token refrescado correctamente."), newRefreshTokenValue);
    }
}
