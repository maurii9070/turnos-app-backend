using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenHandler(TurnosDbContext dbContext, ITokenService tokenService)
{
    public async Task<ApiResponse<RefreshTokenResponse>> HandleAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken is null)
        {
            return ApiResponse<RefreshTokenResponse>.Fail("Refresh token inválido.");
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return ApiResponse<RefreshTokenResponse>.Fail("Refresh token expirado.");
        }

        var accessToken = tokenService.GenerateAccessToken(refreshToken.User);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();

        dbContext.RefreshTokens.Remove(refreshToken);

        var newRefreshToken = new Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = refreshToken.User.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new RefreshTokenResponse(accessToken, newRefreshTokenValue, refreshToken.User.Role.ToString());

        return ApiResponse<RefreshTokenResponse>.Ok(response, "Token refrescado correctamente.");
    }
}
