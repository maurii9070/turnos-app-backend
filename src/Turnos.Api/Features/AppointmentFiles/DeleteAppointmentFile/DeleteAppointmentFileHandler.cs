using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Helpers;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.AppointmentFiles.DeleteAppointmentFile;

public class DeleteAppointmentFileHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<DeleteAppointmentFileResponse>> HandleAsync(
        Guid appointmentId,
        Guid fileId,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<DeleteAppointmentFileResponse>.Fail("Usuario no autenticado.");
        }

        var file = await dbContext.AppointmentFiles
            .AsTracking()
            .Include(f => f.Appointment)
                .ThenInclude(a => a.Patient)
            .Include(f => f.Appointment)
                .ThenInclude(a => a.Payment)
            .FirstOrDefaultAsync(
                f => f.Id == fileId && f.AppointmentId == appointmentId && f.Appointment.IsActive,
                cancellationToken);

        if (file is null)
        {
            return ApiResponse<DeleteAppointmentFileResponse>.Fail("Archivo no encontrado.");
        }

        var isPatient = file.Appointment.Patient.UserId == userId;
        var isDoctor = await AppointmentAuthorization.IsDoctorOfAppointmentAsync(
            appointmentId, currentUser, dbContext, cancellationToken);
        var isAdmin = currentUser.IsInRole(nameof(UserRole.Admin)) ||
                      currentUser.IsInRole(nameof(UserRole.SuperAdmin));

        if (!isPatient && !isDoctor && !isAdmin)
        {
            return ApiResponse<DeleteAppointmentFileResponse>.Fail("No tiene permiso para eliminar este archivo.");
        }

        var wasPatientReceipt = false;

        if (isPatient)
        {
            if (file.Category != AppointmentFileCategory.Receipt)
            {
                return ApiResponse<DeleteAppointmentFileResponse>.Fail("No tiene permiso para eliminar este archivo.");
            }

            wasPatientReceipt = true;

            if (file.Appointment.Status == AppointmentStatus.PendingReview)
            {
                file.Appointment.Status = AppointmentStatus.PendingPayment;
                file.Appointment.UpdatedAt = DateTime.UtcNow;
            }

            if (file.Appointment.Payment is not null &&
                file.Appointment.Payment.ReceiptUrl == file.FilePathOrUrl)
            {
                file.Appointment.Payment.ReceiptUrl = null;
                file.Appointment.Payment.UpdatedAt = DateTime.UtcNow;
            }
        }

        if (isDoctor && file.Category != AppointmentFileCategory.Medical)
        {
            return ApiResponse<DeleteAppointmentFileResponse>.Fail("No tiene permiso para eliminar este archivo.");
        }

        dbContext.AppointmentFiles.Remove(file);
        await dbContext.SaveChangesAsync(cancellationToken);

        var message = wasPatientReceipt
            ? "Comprobante eliminado correctamente. El turno vuelve a estar pendiente de pago."
            : "Archivo eliminado correctamente.";

        return ApiResponse<DeleteAppointmentFileResponse>.Ok(
            new DeleteAppointmentFileResponse(fileId, appointmentId), message);
    }
}
