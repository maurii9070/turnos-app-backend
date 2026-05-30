using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.ListAppointments;

public class ListAppointmentsHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<ListAppointmentsResponse>>> HandleAsync(
        string? statusFilter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.IsActive);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            if (Enum.TryParse<AppointmentStatus>(statusFilter, ignoreCase: true, out var status))
            {
                query = query.Where(a => a.Status == status);
            }
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
            .Select(a => new ListAppointmentsResponse(
                a.Id,
                a.PatientId,
                a.Patient.User.FirstName,
                a.Patient.User.LastName,
                a.Patient.User.Dni,
                a.DoctorId,
                a.Doctor.User.FirstName,
                a.Doctor.User.LastName,
                a.Doctor.Specialty.Name,
                a.Date.ToString("yyyy-MM-dd"),
                a.StartTime.ToString("HH:mm"),
                a.Status.ToString(),
                a.Notes,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ListAppointmentsResponse>>.Ok(appointments);
    }
}
