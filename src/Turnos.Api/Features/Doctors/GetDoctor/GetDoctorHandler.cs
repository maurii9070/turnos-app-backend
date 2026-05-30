using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;

namespace Turnos.Api.Features.Doctors.GetDoctor;

public class GetDoctorHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<GetDoctorResponse>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.Doctors
            .AsNoTracking()
            .Where(d => d.Id == id && d.IsActive && d.User.IsActive)
            .Select(d => new GetDoctorResponse(
                d.Id,
                d.UserId,
                d.User.Dni,
                d.User.FirstName,
                d.User.LastName,
                d.User.Email,
                d.User.Phone,
                new SpecialtyInfo(d.SpecialtyId, d.Specialty.Name),
                d.LicenseNumber,
                d.ConsultationPrice,
                d.Schedules
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new DoctorScheduleResponse(
                        s.Id,
                        s.DayOfWeek.ToString(),
                        s.StartTime,
                        s.EndTime))
                    .ToList(),
                d.Availabilities
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.StartTime)
                    .Select(a => new DoctorAvailabilityResponse(
                        a.Id,
                        a.Date.ToString("yyyy-MM-dd"),
                        a.StartTime,
                        a.EndTime,
                        a.IsAvailable))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (doctor is null)
        {
            return ApiResponse<GetDoctorResponse>.Fail("Doctor no encontrado.");
        }

        return ApiResponse<GetDoctorResponse>.Ok(doctor);
    }
}
