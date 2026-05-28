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
        var response = new RefreshTokenResponse(accessToken, refreshToken.User.Role.ToString());

        return ApiResponse<RefreshTokenResponse>.Ok(response, "Token refrescado correctamente.");
    }
}
