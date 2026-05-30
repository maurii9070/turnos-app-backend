using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Doctors.CreateDoctor;

public class CreateDoctorHandler(TurnosDbContext dbContext, IPasswordHasher passwordHasher)
{
    public async Task<ApiResponse<CreateDoctorResponse>> HandleAsync(
        CreateDoctorRequest request,
        CancellationToken cancellationToken)
    {
        var dniExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Dni == request.Dni, cancellationToken);

        if (dniExists)
        {
            return ApiResponse<CreateDoctorResponse>.Fail("Ya existe un usuario con ese DNI.");
        }

        var licenseExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.LicenseNumber == request.LicenseNumber, cancellationToken);

        if (licenseExists)
        {
            return ApiResponse<CreateDoctorResponse>.Fail("Ya existe un doctor con ese número de licencia.");
        }

        var specialty = await dbContext.Specialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SpecialtyId && s.IsActive, cancellationToken);

        if (specialty is null)
        {
            return ApiResponse<CreateDoctorResponse>.Fail("Especialidad no encontrada.");
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
            Role = UserRole.Doctor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SpecialtyId = request.SpecialtyId,
            LicenseNumber = request.LicenseNumber.Trim(),
            ConsultationPrice = request.ConsultationPrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.Doctors.Add(doctor);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateDoctorResponse(
            doctor.Id,
            user.Id,
            user.Dni,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            specialty.Name,
            doctor.LicenseNumber,
            doctor.ConsultationPrice,
            user.CreatedAt);

        return ApiResponse<CreateDoctorResponse>.Ok(response, "Doctor creado correctamente.");
    }
}
