using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Appointments.GetAppointment;

public class GetAppointmentHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<GetAppointmentResponse>> HandleAsync(
        Guid id,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        if (!await AppointmentAuthorization.CanAccessAppointmentAsync(id, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<GetAppointmentResponse>.Fail("No autorizado.");
        }

        var appointment = await dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
            .Where(a => a.Id == id && a.IsActive)
            .Select(a => new GetAppointmentResponse(
                a.Id,
                a.PatientId,
                a.Patient.User.FirstName,
                a.Patient.User.LastName,
                a.Patient.User.Dni,
                a.DoctorId,
                a.Doctor.User.FirstName,
                a.Doctor.User.LastName,
                a.Doctor.User.Email,
                a.Doctor.LicenseNumber,
                a.Doctor.Specialty.Name,
                a.Doctor.ConsultationPrice,
                a.Date.ToString("yyyy-MM-dd"),
                a.StartTime.ToString("HH:mm"),
                a.Status.ToString(),
                a.Notes,
                a.CreatedAt,
                a.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<GetAppointmentResponse>.Fail("Turno no encontrado.");
        }

        return ApiResponse<GetAppointmentResponse>.Ok(appointment);
    }
}
