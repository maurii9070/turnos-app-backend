using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.CreateAppointmentWithMercadoPago;

public class CreateAppointmentWithMercadoPagoHandler(
    TurnosDbContext dbContext,
    IMercadoPagoService mercadoPagoService,
    ILogger<CreateAppointmentWithMercadoPagoHandler> logger)
{
    public async Task<ApiResponse<CreateAppointmentWithMercadoPagoResponse>> HandleAsync(
        CreateAppointmentWithMercadoPagoRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("Usuario no autenticado.");
        }

        var patient = await dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken);

        if (patient is null)
        {
            return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("Paciente no encontrado.");
        }

        var doctor = await dbContext.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.IsActive && d.User.IsActive, cancellationToken);

        if (doctor is null)
        {
            return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("Doctor no encontrado.");
        }

        if (request.Date <= DateOnly.FromDateTime(DateTime.UtcNow) && request.Date < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("La fecha debe ser posterior a hoy.");
        }

        var startTime = request.StartTime;
        var endTime = startTime.AddMinutes(30);

        var availability = await dbContext.DoctorAvailabilities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.DoctorId == doctor.Id && a.Date == request.Date && a.IsActive, cancellationToken);

        if (availability is not null)
        {
            if (!availability.IsAvailable)
            {
                return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("El doctor no atiende en esa fecha.");
            }

            if (startTime < availability.StartTime || endTime > availability.EndTime)
            {
                return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("El horario solicitado no está dentro de la disponibilidad del doctor.");
            }
        }
        else
        {
            var dayOfWeek = request.Date.DayOfWeek;
            var schedule = await dbContext.Schedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctor.Id && s.DayOfWeek == dayOfWeek && s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (schedule is null)
            {
                return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("El doctor no tiene horario disponible en esa fecha.");
            }

            if (startTime < schedule.StartTime || endTime > schedule.EndTime)
            {
                return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("El horario solicitado no está dentro del horario del doctor.");
            }
        }

        var conflict = await dbContext.Appointments.AnyAsync(
            a => a.DoctorId == doctor.Id &&
                 a.Date == request.Date &&
                 a.StartTime == startTime &&
                 a.Status != AppointmentStatus.Cancelled,
            cancellationToken);

        if (conflict)
        {
            return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("El horario solicitado ya está ocupado.");
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            Date = request.Date,
            StartTime = startTime,
            Status = AppointmentStatus.PendingPayment,
            Notes = request.Notes?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointment.Id,
            Amount = doctor.ConsultationPrice,
            Method = PaymentMethod.MercadoPago,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            var preferenceResult = await mercadoPagoService.CreatePreferenceAsync(
                payment.Id,
                payment.Amount,
                $"Turno médico - {doctor.User.FirstName} {doctor.User.LastName}",
                request.PayerEmail ?? doctor.User.Email ?? string.Empty,
                request.PayerFirstName ?? doctor.User.FirstName,
                request.PayerLastName ?? doctor.User.LastName,
                cancellationToken);

            payment.PreferenceId = preferenceResult.PreferenceId;
            payment.ExternalReference = payment.Id.ToString();

            dbContext.Appointments.Add(appointment);
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync(cancellationToken);

            return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Ok(
                new CreateAppointmentWithMercadoPagoResponse(
                    appointment.Id,
                    payment.Id,
                    payment.Amount,
                    preferenceResult.InitPoint,
                    preferenceResult.PreferenceId),
                "Turno creado. Redirigiendo a Mercado Pago.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear preferencia de Mercado Pago");
            return ApiResponse<CreateAppointmentWithMercadoPagoResponse>.Fail("Error al crear la preferencia de Mercado Pago. Inténtelo nuevamente.");
        }
    }
}