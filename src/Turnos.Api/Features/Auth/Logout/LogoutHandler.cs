using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Auth.Logout;

public sealed class LogoutHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<object>> HandleAsync(string refreshTokenValue, CancellationToken cancellationToken)
    {
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue, cancellationToken);

        if (refreshToken is not null)
        {
            dbContext.RefreshTokens.Remove(refreshToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<object>.Ok(new { }, "Sesión cerrada correctamente.");
    }
}
