using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Patients.GetPatientByDni;

public class GetPatientByDniHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<GetPatientByDniResponse>> HandleAsync(
        string dni,
        CancellationToken cancellationToken)
    {
        var patient = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Dni == dni && u.Role == UserRole.Patient)
            .Select(u => new
            {
                u.Id,
                u.Dni,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Phone,
                u.IsActive,
                u.CreatedAt,
                u.Patient,
                TotalAppointments = u.Patient != null
                    ? dbContext.Appointments.Count(a => a.PatientId == u.Patient.Id)
                    : 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (patient is null)
        {
            return ApiResponse<GetPatientByDniResponse>.Fail("Paciente no encontrado.");
        }

        var response = new GetPatientByDniResponse(
            patient.Patient!.Id,
            patient.Id,
            patient.Dni,
            patient.FirstName,
            patient.LastName,
            patient.Email,
            patient.Phone,
            patient.Patient.DateOfBirth,
            patient.IsActive,
            patient.TotalAppointments,
            patient.CreatedAt);

        return ApiResponse<GetPatientByDniResponse>.Ok(response);
    }
}
