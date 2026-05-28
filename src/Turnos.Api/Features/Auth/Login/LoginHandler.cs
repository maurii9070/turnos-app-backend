using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Auth.Login;

public sealed class LoginHandler(TurnosDbContext dbContext, IPasswordHasher passwordHasher, ITokenService tokenService)
{
    public async Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> HandleAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Dni == request.Dni, cancellationToken);

        if (user is null)
        {
            return (ApiResponse<LoginResponse>.Fail("Credenciales inválidas."), null);
        }

        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return (ApiResponse<LoginResponse>.Fail("Credenciales inválidas."), null);
        }

        if (!user.IsActive)
        {
            return (ApiResponse<LoginResponse>.Fail("Usuario deshabilitado."), null);
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        var refreshToken = new Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(accessToken, user.Role.ToString());
        return (ApiResponse<LoginResponse>.Ok(response, "Inicio de sesión exitoso."), refreshTokenValue);
    }
}
