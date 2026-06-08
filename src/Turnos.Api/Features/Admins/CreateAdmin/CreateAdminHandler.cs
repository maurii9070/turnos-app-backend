using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Admins.CreateAdmin;

public class CreateAdminHandler(TurnosDbContext dbContext, IPasswordHasher passwordHasher)
{
    public async Task<ApiResponse<CreateAdminResponse>> HandleAsync(
        CreateAdminRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var registeredByUserId))
        {
            return ApiResponse<CreateAdminResponse>.Fail("Usuario no autenticado.");
        }

        var dniExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Dni == request.Dni, cancellationToken);

        if (dniExists)
        {
            return ApiResponse<CreateAdminResponse>.Fail("Ya existe un usuario con ese DNI.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Dni = request.Dni.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = UserRole.Admin,
            RegisteredBy = registeredByUserId,
            MustChangePassword = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateAdminResponse(
            user.Id,
            user.Dni,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.MustChangePassword,
            user.CreatedAt);

        return ApiResponse<CreateAdminResponse>.Ok(
            response,
            "Administrador registrado correctamente. Deberá cambiar su contraseña al iniciar sesión.");
    }
}
