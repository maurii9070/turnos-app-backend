using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.UpdatePaymentStatus;

public class UpdatePaymentStatusHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<UpdatePaymentStatusResponse>> HandleAsync(
        Guid paymentId,
        UpdatePaymentStatusRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .AsTracking()
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Patient)
                    .ThenInclude(p => p.User)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return ApiResponse<UpdatePaymentStatusResponse>.Fail("Pago no encontrado.");
        }

        var isAdmin = currentUser.IsInRole(nameof(UserRole.Admin)) ||
                      currentUser.IsInRole(nameof(UserRole.SuperAdmin));
        var isDoctor = await AppointmentAuthorization.IsDoctorOfAppointmentAsync(
            payment.AppointmentId, currentUser, dbContext, cancellationToken);

        if (!isAdmin && !isDoctor)
        {
            return ApiResponse<UpdatePaymentStatusResponse>.Fail("No tiene permiso para modificar este pago.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return ApiResponse<UpdatePaymentStatusResponse>.Fail("Solo se pueden modificar pagos pendientes.");
        }

        payment.Status = request.Status;
        payment.PaidAt = request.Status == PaymentStatus.Approved ? DateTime.UtcNow : null;
        payment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdatePaymentStatusResponse(
            payment.Id,
            payment.AppointmentId,
            payment.Amount,
            payment.Method.ToString(),
            payment.Status.ToString(),
            payment.ReceiptUrl,
            payment.PaidAt,
            payment.UpdatedAt);

        return ApiResponse<UpdatePaymentStatusResponse>.Ok(response, "Pago actualizado correctamente.");
    }
}
