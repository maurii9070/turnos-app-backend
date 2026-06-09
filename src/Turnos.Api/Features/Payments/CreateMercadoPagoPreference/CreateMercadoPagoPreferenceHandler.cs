using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.CreateMercadoPagoPreference;

public class CreateMercadoPagoPreferenceHandler(
    TurnosDbContext dbContext,
    IMercadoPagoService mercadoPagoService)
{
    public async Task<ApiResponse<CreateMercadoPagoPreferenceResponse>> HandleAsync(
        Guid appointmentId,
        CreateMercadoPagoPreferenceRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("Usuario no autenticado.");
        }

        var appointment = await dbContext.Appointments
            .AsTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Payment)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.IsActive, cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("Turno no encontrado.");
        }

        if (appointment.Patient.UserId != userId)
        {
            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("No tiene permiso para pagar este turno.");
        }

        if (appointment.Status != AppointmentStatus.PendingPayment)
        {
            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("Solo se pueden pagar turnos en estado pendiente de pago.");
        }

        if (appointment.Payment is not null && appointment.Payment.Status == PaymentStatus.Approved)
        {
            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("Este turno ya tiene un pago aprobado.");
        }

        if (appointment.Payment?.Status == PaymentStatus.Pending && appointment.Payment.Method == PaymentMethod.MercadoPago)
        {
            try
            {
                var existingResult = await mercadoPagoService.CreatePreferenceAsync(
                    appointment.Payment.Id,
                    appointment.Payment.Amount,
                    $"Turno médico - {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                    request.PayerEmail ?? appointment.Patient.User.Email ?? string.Empty,
                    request.PayerFirstName ?? appointment.Patient.User.FirstName,
                    request.PayerLastName ?? appointment.Patient.User.LastName,
                    cancellationToken);

                appointment.Payment.PreferenceId = existingResult.PreferenceId;
                appointment.Payment.ExternalReference = appointment.Payment.Id.ToString();
                appointment.Payment.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                return ApiResponse<CreateMercadoPagoPreferenceResponse>.Ok(
                    new CreateMercadoPagoPreferenceResponse(
                        appointment.Payment.Id,
                        appointment.Payment.AppointmentId,
                        appointment.Payment.Amount,
                        appointment.Payment.Status.ToString(),
                        existingResult.PreferenceId,
                        existingResult.InitPoint),
                    "Preferencia de Mercado Pago creada.");
            }
            catch
            {
                return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("Error al crear la preferencia de Mercado Pago. Inténtelo nuevamente.");
            }
        }

        if (appointment.Payment is not null && appointment.Payment.Method == PaymentMethod.MercadoPago && appointment.Payment.Status == PaymentStatus.Rejected)
        {
            dbContext.Payments.Remove(appointment.Payment);
        }

        if (appointment.Payment is not null && appointment.Payment.Method != PaymentMethod.MercadoPago && appointment.Payment.Status == PaymentStatus.Pending)
        {
            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("Este turno ya tiene un pago pendiente con otro medio de pago. Cancele el pago existente antes de usar Mercado Pago.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointment.Id,
            Amount = appointment.Doctor.ConsultationPrice,
            Method = PaymentMethod.MercadoPago,
            Status = PaymentStatus.Pending,
            ExternalReference = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            var preferenceResult = await mercadoPagoService.CreatePreferenceAsync(
                payment.Id,
                payment.Amount,
                $"Turno médico - {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                request.PayerEmail ?? appointment.Patient.User.Email ?? string.Empty,
                request.PayerFirstName ?? appointment.Patient.User.FirstName,
                request.PayerLastName ?? appointment.Patient.User.LastName,
                cancellationToken);

            payment.PreferenceId = preferenceResult.PreferenceId;
            payment.ExternalReference = payment.Id.ToString();

            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync(cancellationToken);

            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Ok(
                new CreateMercadoPagoPreferenceResponse(
                    payment.Id,
                    payment.AppointmentId,
                    payment.Amount,
                    payment.Status.ToString(),
                    preferenceResult.PreferenceId,
                    preferenceResult.InitPoint),
                "Preferencia de Mercado Pago creada.");
        }
        catch (Exception)
        {
            return ApiResponse<CreateMercadoPagoPreferenceResponse>.Fail("Error al crear la preferencia de Mercado Pago. Inténtelo nuevamente.");
        }
    }
}