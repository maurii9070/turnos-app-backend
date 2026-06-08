using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Patients.CreatePatient;

public class CreatePatientHandler(TurnosDbContext dbContext, IPasswordHasher passwordHasher)
{
    public async Task<ApiResponse<CreatePatientResponse>> HandleAsync(
        CreatePatientRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var registeredByUserId))
        {
            return ApiResponse<CreatePatientResponse>.Fail("Usuario no autenticado.");
        }

        var dniExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Dni == request.Dni, cancellationToken);

        if (dniExists)
        {
            return ApiResponse<CreatePatientResponse>.Fail("Ya existe un usuario con ese DNI.");
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
            Role = UserRole.Patient,
            RegisteredBy = registeredByUserId,
            MustChangePassword = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DateOfBirth = request.DateOfBirth,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreatePatientResponse(
            patient.Id,
            user.Id,
            user.Dni,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.MustChangePassword,
            user.CreatedAt);

        return ApiResponse<CreatePatientResponse>.Ok(
            response,
            "Paciente registrado correctamente. Deberá cambiar su contraseña al iniciar sesión.");
    }
}
