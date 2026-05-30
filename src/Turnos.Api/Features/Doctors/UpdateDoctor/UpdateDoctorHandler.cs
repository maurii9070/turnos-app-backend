using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.UpdateDoctor;

public class UpdateDoctorHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<UpdateDoctorResponse>> HandleAsync(
        Guid id,
        UpdateDoctorRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        if (!await DoctorAuthorization.CanManageDoctorAsync(id, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<UpdateDoctorResponse>.Fail("No autorizado.");
        }

        var doctor = await dbContext.Doctors
            .AsTracking()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive && d.User.IsActive, cancellationToken);

        if (doctor is null)
        {
            return ApiResponse<UpdateDoctorResponse>.Fail("Doctor no encontrado.");
        }

        var newSpecialty = await dbContext.Specialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SpecialtyId && s.IsActive, cancellationToken);

        if (newSpecialty is null)
        {
            return ApiResponse<UpdateDoctorResponse>.Fail("Especialidad no encontrada.");
        }

        var duplicateLicense = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.LicenseNumber == request.LicenseNumber && d.Id != id, cancellationToken);

        if (duplicateLicense)
        {
            return ApiResponse<UpdateDoctorResponse>.Fail("Ya existe un doctor con ese número de licencia.");
        }

        doctor.SpecialtyId = request.SpecialtyId;
        doctor.LicenseNumber = request.LicenseNumber.Trim();
        doctor.ConsultationPrice = request.ConsultationPrice;
        doctor.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateDoctorResponse(
            doctor.Id,
            doctor.UserId,
            doctor.User.Dni,
            doctor.User.FirstName,
            doctor.User.LastName,
            newSpecialty.Name,
            doctor.LicenseNumber,
            doctor.ConsultationPrice);

        return ApiResponse<UpdateDoctorResponse>.Ok(response, "Doctor actualizado correctamente.");
    }
}
