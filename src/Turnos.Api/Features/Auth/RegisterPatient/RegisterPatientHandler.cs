using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Auth.RegisterPatient;

public sealed class RegisterPatientHandler(TurnosDbContext dbContext, IPasswordHasher passwordHasher)
{
    public async Task<ApiResponse<RegisterPatientResponse>> HandleAsync(
        RegisterPatientRequest request,
        CancellationToken cancellationToken)
    {
        var dniExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Dni == request.Dni, cancellationToken);

        if (dniExists)
        {
            return ApiResponse<RegisterPatientResponse>.Fail("Ya existe un usuario con ese DNI.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Dni = request.Dni.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            Role = UserRole.Patient,
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

        var response = new RegisterPatientResponse(
            user.Id,
            user.Dni,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.CreatedAt);

        return ApiResponse<RegisterPatientResponse>.Ok(response, "Paciente registrado correctamente.");
    }
}
