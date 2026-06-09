using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.SyncMercadoPagoPayment;

public class SyncMercadoPagoPaymentHandler(
    TurnosDbContext dbContext,
    IMercadoPagoService mercadoPagoService)
{
    public async Task<ApiResponse<SyncMercadoPagoPaymentResponse>> HandleAsync(
        Guid paymentId,
        ClaimsPrincipal currentUser,
        CancellationToken ct)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out _))
        {
            return ApiResponse<SyncMercadoPagoPaymentResponse>.Fail("Usuario no autenticado.");
        }

        var payment = await dbContext.Payments
            .AsTracking()
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

        if (payment is null)
        {
            return ApiResponse<SyncMercadoPagoPaymentResponse>.Fail("Pago no encontrado.");
        }

        if (payment.Method != PaymentMethod.MercadoPago)
        {
            return ApiResponse<SyncMercadoPagoPaymentResponse>.Fail("El pago no es de Mercado Pago.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return ApiResponse<SyncMercadoPagoPaymentResponse>.Fail("El pago ya fue resuelto.");
        }

        var previousStatus = payment.Status;
        var externalRef = payment.ExternalReference ?? payment.Id.ToString();

        var mpPayment = await mercadoPagoService.SearchPaymentByExternalReferenceAsync(externalRef, ct);
        if (mpPayment is null)
        {
            return ApiResponse<SyncMercadoPagoPaymentResponse>.Fail("No se encontró el pago en Mercado Pago. Espere unos segundos y vuelva a intentar.");
        }

        payment.MercadoPagoPaymentId = mpPayment.Id.ToString();
        payment.UpdatedAt = DateTime.UtcNow;

        var newStatus = MapPaymentStatus(mpPayment.Status);
        var appointmentConfirmed = false;

        if (payment.Status != newStatus)
        {
            payment.Status = newStatus;

            if (newStatus == PaymentStatus.Approved)
            {
                payment.PaidAt = DateTime.UtcNow;

                if (payment.Appointment is not null &&
                    payment.Appointment.Status == AppointmentStatus.PendingPayment)
                {
                    payment.Appointment.Status = AppointmentStatus.Confirmed;
                    payment.Appointment.UpdatedAt = DateTime.UtcNow;
                    appointmentConfirmed = true;
                }
            }
        }

        await dbContext.SaveChangesAsync(ct);

        return ApiResponse<SyncMercadoPagoPaymentResponse>.Ok(
            new SyncMercadoPagoPaymentResponse(
                payment.Id,
                payment.AppointmentId,
                previousStatus.ToString(),
                payment.Status.ToString(),
                appointmentConfirmed),
            appointmentConfirmed ? "Pago aprobado y turno confirmado." : $"Pago en estado: {payment.Status}.");
    }

    private static PaymentStatus MapPaymentStatus(string mpStatus) => mpStatus switch
    {
        "approved" => PaymentStatus.Approved,
        "pending" or "in_process" or "authorized" => PaymentStatus.Pending,
        _ => PaymentStatus.Rejected
    };
}