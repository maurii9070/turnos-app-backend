using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Auth.GoogleLogin;

public sealed class GoogleLoginHandler(
    TurnosDbContext dbContext,
    ISupabaseAuthClient supabaseAuthClient,
    ITokenService tokenService)
{
    public async Task<ApiResponse<GoogleLoginResponse>> HandleAsync(
        GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var supabaseUser = await supabaseAuthClient.GetUserInfoAsync(request.SupabaseToken, cancellationToken);

        if (supabaseUser is null)
        {
            return ApiResponse<GoogleLoginResponse>.Fail("Token de Google inválido.");
        }

        if (!supabaseUser.EmailVerified)
        {
            return ApiResponse<GoogleLoginResponse>.Fail("El email de Google no está verificado.");
        }

        var existingUser = await dbContext.Users
            .AsTracking()
            .Include(u => u.Patient)
            .FirstOrDefaultAsync(u => u.GoogleId == supabaseUser.Id, cancellationToken);

        if (existingUser is not null)
        {
            return await AuthenticateUserAsync(existingUser, supabaseUser, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(supabaseUser.Email))
        {
            var userByEmail = await dbContext.Users
                .AsTracking()
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(
                    u => u.Email == supabaseUser.Email && u.GoogleId == null,
                    cancellationToken);

            if (userByEmail is not null)
            {
                userByEmail.GoogleId = supabaseUser.Id;
                userByEmail.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                return await AuthenticateUserAsync(userByEmail, supabaseUser, cancellationToken);
            }
        }

        return await CreateGoogleUserAsync(supabaseUser, cancellationToken);
    }

    private async Task<ApiResponse<GoogleLoginResponse>> AuthenticateUserAsync(
        User user,
        Common.Security.SupabaseUserInfo supabaseUser,
        CancellationToken cancellationToken)
    {
        if (!user.IsActive)
        {
            return ApiResponse<GoogleLoginResponse>.Fail("Usuario deshabilitado.");
        }

        if (supabaseUser.FirstName is not null && supabaseUser.LastName is not null)
        {
            user.FirstName = supabaseUser.FirstName;
            user.LastName = supabaseUser.LastName;
            user.UpdatedAt = DateTime.UtcNow;
        }

        if (supabaseUser.Email is not null && user.Email != supabaseUser.Email)
        {
            user.Email = supabaseUser.Email;
            user.UpdatedAt = DateTime.UtcNow;
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        var oldTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync(cancellationToken);

        if (oldTokens.Count != 0)
        {
            dbContext.RefreshTokens.RemoveRange(oldTokens);
        }

        var refreshToken = new Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new GoogleLoginResponse(accessToken, refreshTokenValue, user.Role.ToString());
        return ApiResponse<GoogleLoginResponse>.Ok(response, "Inicio de sesión exitoso.");
    }

    private async Task<ApiResponse<GoogleLoginResponse>> CreateGoogleUserAsync(
        Common.Security.SupabaseUserInfo supabaseUser,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            GoogleId = supabaseUser.Id,
            Email = supabaseUser.Email,
            FirstName = supabaseUser.FirstName ?? "Usuario",
            LastName = supabaseUser.LastName ?? "Google",
            Role = UserRole.Patient,
            RequiresDni = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.Patients.Add(patient);

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        var refreshToken = new Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new GoogleLoginResponse(accessToken, refreshTokenValue, user.Role.ToString());
        return ApiResponse<GoogleLoginResponse>.Ok(response, "Cuenta creada correctamente. Completá tu DNI para continuar.");
    }
}
