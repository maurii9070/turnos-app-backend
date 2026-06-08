using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.CreatePayment;

public class CreatePaymentHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<CreatePaymentResponse>> HandleAsync(
        Guid appointmentId,
        CreatePaymentRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<CreatePaymentResponse>.Fail("Usuario no autenticado.");
        }

        var appointment = await dbContext.Appointments
            .AsTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Payment)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.IsActive, cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<CreatePaymentResponse>.Fail("Turno no encontrado.");
        }

        if (appointment.Patient.UserId != userId)
        {
            return ApiResponse<CreatePaymentResponse>.Fail("No tiene permiso para pagar este turno.");
        }

        if (appointment.Status != AppointmentStatus.PendingPayment)
        {
            return ApiResponse<CreatePaymentResponse>.Fail("Solo se pueden pagar turnos en estado pendiente de pago.");
        }

        if (appointment.Payment is not null)
        {
            if (appointment.Payment.Status == PaymentStatus.Pending)
            {
                var existingResponse = new CreatePaymentResponse(
                    appointment.Payment.Id,
                    appointment.Payment.AppointmentId,
                    appointment.Payment.Amount,
                    appointment.Payment.Method.ToString(),
                    appointment.Payment.Status.ToString(),
                    appointment.Payment.ReceiptUrl,
                    appointment.Payment.CreatedAt);
                return ApiResponse<CreatePaymentResponse>.Ok(existingResponse, "Pago ya registrado.");
            }

            if (appointment.Payment.Status == PaymentStatus.Approved)
            {
                return ApiResponse<CreatePaymentResponse>.Fail("Este turno ya tiene un pago aprobado.");
            }

            dbContext.Payments.Remove(appointment.Payment);
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointment.Id,
            Amount = appointment.Doctor.ConsultationPrice,
            Method = request.Method,
            Status = PaymentStatus.Pending,
            ReceiptUrl = request.ReceiptUrl?.Trim(),
            PaidAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        appointment.Status = AppointmentStatus.PendingPayment;
        appointment.UpdatedAt = DateTime.UtcNow;

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var message = "Pago registrado. Debe subir el comprobante para continuar con la revisión.";

        var response = new CreatePaymentResponse(
            payment.Id,
            payment.AppointmentId,
            payment.Amount,
            payment.Method.ToString(),
            payment.Status.ToString(),
            payment.ReceiptUrl,
            payment.CreatedAt);

        return ApiResponse<CreatePaymentResponse>.Ok(response, message);
    }
}
