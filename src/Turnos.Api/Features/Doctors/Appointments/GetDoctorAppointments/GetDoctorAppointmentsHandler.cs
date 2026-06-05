using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Doctors.Appointments.GetDoctorAppointments;

public class GetDoctorAppointmentsHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<List<GetDoctorAppointmentsResponse>>> HandleAsync(
        Guid doctorId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == doctorId && d.IsActive, cancellationToken);

        if (!doctorExists)
        {
            return ApiResponse<List<GetDoctorAppointmentsResponse>>.Fail("Doctor no encontrado.");
        }

        var appointments = await dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.Date == date && a.IsActive)
            .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.StartTime)
            .Select(a => new GetDoctorAppointmentsResponse(
                a.Id,
                a.Date.ToString("yyyy-MM-dd"),
                a.StartTime.ToString("HH:mm"),
                a.Status.ToString()))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<GetDoctorAppointmentsResponse>>.Ok(appointments);
    }
}
