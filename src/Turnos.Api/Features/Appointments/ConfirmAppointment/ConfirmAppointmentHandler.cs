using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.ConfirmAppointment;

public class ConfirmAppointmentHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<ConfirmAppointmentResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var appointment = await dbContext.Appointments
            .AsTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<ConfirmAppointmentResponse>.Fail("Turno no encontrado.");
        }

        if (appointment.Status != AppointmentStatus.PendingPayment &&
            appointment.Status != AppointmentStatus.PendingReview)
        {
            return ApiResponse<ConfirmAppointmentResponse>.Fail(
                "Solo se pueden confirmar turnos pendientes de pago o pendientes de revisión.");
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new ConfirmAppointmentResponse(appointment.Id, appointment.Status.ToString());
        return ApiResponse<ConfirmAppointmentResponse>.Ok(response, "Turno confirmado correctamente.");
    }
}
