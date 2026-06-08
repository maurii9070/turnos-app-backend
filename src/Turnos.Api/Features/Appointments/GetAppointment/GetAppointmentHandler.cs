using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

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
            .AsSplitQuery()
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
            .Include(a => a.Files)
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<GetAppointmentResponse>.Fail("Turno no encontrado.");
        }

        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = userIdClaim is not null && Guid.TryParse(userIdClaim, out var userId) && appointment.Patient.UserId == userId;
        var isDoctor = await AppointmentAuthorization.IsDoctorOfAppointmentAsync(id, currentUser, dbContext, cancellationToken);
        var isAdmin = currentUser.IsInRole(nameof(UserRole.Admin)) || currentUser.IsInRole(nameof(UserRole.SuperAdmin));

        var files = appointment.Files
            .OrderBy(f => f.UploadedAt)
            .Where(f => isPatient || (isDoctor && f.Category == AppointmentFileCategory.Medical) || (isAdmin && f.Category == AppointmentFileCategory.Receipt))
            .Select(f => new AppointmentFileResponse(
                f.Id,
                f.AppointmentId,
                f.FilePathOrUrl,
                f.FileName,
                f.FileType,
                f.Category.ToString(),
                f.UploadedAt))
            .ToList();

        var response = new GetAppointmentResponse(
            appointment.Id,
            appointment.PatientId,
            appointment.Patient.User.FirstName,
            appointment.Patient.User.LastName,
            appointment.Patient.User.Dni,
            appointment.DoctorId,
            appointment.Doctor.User.FirstName,
            appointment.Doctor.User.LastName,
            appointment.Doctor.User.Email,
            appointment.Doctor.LicenseNumber,
            appointment.Doctor.Specialty?.Name,
            appointment.Doctor.ConsultationPrice,
            appointment.Date.ToString("yyyy-MM-dd"),
            appointment.StartTime.ToString("HH:mm"),
            appointment.Status.ToString(),
            appointment.Notes,
            appointment.CreatedAt,
            appointment.UpdatedAt,
            files);

        return ApiResponse<GetAppointmentResponse>.Ok(response);
    }
}
