using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.CancelAppointment;

public class CancelAppointmentHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<CancelAppointmentResponse>> HandleAsync(
        Guid id,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var appointment = await dbContext.Appointments
            .AsTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<CancelAppointmentResponse>.Fail("Turno no encontrado.");
        }

        if (!await AppointmentAuthorization.CanAccessAppointmentAsync(id, currentUser, dbContext, cancellationToken))
        {
            return ApiResponse<CancelAppointmentResponse>.Fail("No autorizado.");
        }

        if (appointment.Status != AppointmentStatus.PendingPayment)
        {
            return ApiResponse<CancelAppointmentResponse>.Fail("Solo se pueden cancelar turnos pendientes de pago.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CancelAppointmentResponse(appointment.Id, appointment.Status.ToString());
        return ApiResponse<CancelAppointmentResponse>.Ok(response, "Turno cancelado correctamente.");
    }
}
