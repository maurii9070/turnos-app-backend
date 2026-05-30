using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.CompleteAppointment;

public class CompleteAppointmentHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<CompleteAppointmentResponse>> HandleAsync(
        Guid id,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var appointment = await dbContext.Appointments
            .AsTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<CompleteAppointmentResponse>.Fail("Turno no encontrado.");
        }

        if (!await AppointmentAuthorization.IsDoctorOfAppointmentAsync(id, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<CompleteAppointmentResponse>.Fail("No autorizado.");
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            return ApiResponse<CompleteAppointmentResponse>.Fail("Solo se pueden completar turnos confirmados.");
        }

        appointment.Status = AppointmentStatus.Completed;
        appointment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CompleteAppointmentResponse(appointment.Id, appointment.Status.ToString());
        return ApiResponse<CompleteAppointmentResponse>.Ok(response, "Turno completado correctamente.");
    }
}
