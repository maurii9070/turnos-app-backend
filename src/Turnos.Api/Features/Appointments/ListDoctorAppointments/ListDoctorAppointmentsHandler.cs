using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.ListDoctorAppointments;

public class ListDoctorAppointmentsHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<ListDoctorAppointmentsResponse>>> HandleAsync(
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<List<ListDoctorAppointmentsResponse>>.Fail("Usuario no autenticado.");
        }

        if (!currentUser.IsInRole(nameof(UserRole.Doctor)))
        {
            return ApiResponse<List<ListDoctorAppointmentsResponse>>.Fail("Solo un doctor puede consultar estos turnos.");
        }

        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.UserId == userId && d.IsActive, cancellationToken);

        if (!doctorExists)
        {
            return ApiResponse<List<ListDoctorAppointmentsResponse>>.Fail("Doctor no encontrado.");
        }

        var appointments = await dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Doctor.UserId == userId && a.IsActive)
            .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.StartTime)
            .Select(a => new ListDoctorAppointmentsResponse(
                a.Id,
                a.DoctorId,
                a.Doctor.User.FirstName,
                a.Doctor.User.LastName,
                a.Doctor.Specialty.Name,
                a.PatientId,
                a.Patient.User.FirstName,
                a.Patient.User.LastName,
                a.Date.ToString("yyyy-MM-dd"),
                a.StartTime.ToString("HH:mm"),
                a.Status.ToString(),
                a.Notes))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ListDoctorAppointmentsResponse>>.Ok(appointments);
    }
}