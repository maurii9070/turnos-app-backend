using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.ListMyAppointments;

public class ListMyAppointmentsHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<ListMyAppointmentsResponse>>> HandleAsync(
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<List<ListMyAppointmentsResponse>>.Fail("Usuario no autenticado.");
        }

        IQueryable<Entities.Appointment> query;

        if (currentUser.IsInRole(nameof(UserRole.Doctor)))
        {
            query = dbContext.Appointments
                .AsNoTracking()
                .Where(a => a.Doctor.UserId == userId && a.IsActive);
        }
        else
        {
            query = dbContext.Appointments
                .AsNoTracking()
                .Where(a => a.Patient.UserId == userId && a.IsActive);
        }

        var appointments = await query
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialty)
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.StartTime)
            .Select(a => new ListMyAppointmentsResponse(
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

        return ApiResponse<List<ListMyAppointmentsResponse>>.Ok(appointments);
    }
}
