using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.AppointmentFiles.UploadAppointmentFile;

public class UploadAppointmentFileHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<UploadAppointmentFileResponse>> HandleAsync(
        Guid appointmentId,
        UploadAppointmentFileRequest request,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<UploadAppointmentFileResponse>.Fail("Usuario no autenticado.");
        }

        var appointment = await dbContext.Appointments
            .AsTracking()
            .Include(a => a.Patient)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.IsActive, cancellationToken);

        if (appointment is null)
        {
            return ApiResponse<UploadAppointmentFileResponse>.Fail("Turno no encontrado.");
        }

        var isPatient = appointment.Patient.UserId == userId;
        var isDoctor = await AppointmentAuthorization.IsDoctorOfAppointmentAsync(
            appointmentId, currentUser, dbContext, cancellationToken);
        var isAdmin = currentUser.IsInRole(nameof(UserRole.Admin)) ||
                      currentUser.IsInRole(nameof(UserRole.SuperAdmin));

        if (!isPatient && !isDoctor && !isAdmin)
        {
            return ApiResponse<UploadAppointmentFileResponse>.Fail("No tiene permiso para subir archivos a este turno.");
        }

        var category = isPatient ? AppointmentFileCategory.Receipt : AppointmentFileCategory.Medical;

        var appointmentFile = new AppointmentFile
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointment.Id,
            FileName = request.FileName.Trim(),
            FileType = request.FileType.Trim(),
            FilePathOrUrl = request.FilePathOrUrl.Trim(),
            Category = category,
            UploadedAt = DateTime.UtcNow
        };

        appointment.UpdatedAt = DateTime.UtcNow;

        var transitionedToReview = false;

        if (isPatient && appointment.Status == AppointmentStatus.PendingPayment)
        {
            appointment.Status = AppointmentStatus.PendingReview;
            transitionedToReview = true;

            if (appointment.Payment is not null && appointment.Payment.Status == PaymentStatus.Pending)
            {
                appointment.Payment.ReceiptUrl = request.FilePathOrUrl.Trim();
                appointment.Payment.UpdatedAt = DateTime.UtcNow;
            }
        }

        dbContext.AppointmentFiles.Add(appointmentFile);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UploadAppointmentFileResponse(
            appointmentFile.Id,
            appointmentFile.AppointmentId,
            appointmentFile.FileName,
            appointmentFile.FileType,
            appointmentFile.FilePathOrUrl,
            appointmentFile.Category.ToString(),
            appointmentFile.UploadedAt);

        var message = transitionedToReview
            ? "Comprobante subido correctamente. El turno queda pendiente de revisión."
            : "Archivo subido correctamente.";

        return ApiResponse<UploadAppointmentFileResponse>.Ok(response, message);
    }
}
