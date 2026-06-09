using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.GetMercadoPagoPaymentStatus;

public class GetMercadoPagoPaymentStatusHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<GetMercadoPagoPaymentStatusResponse>> HandleAsync(
        Guid paymentId,
        ClaimsPrincipal currentUser,
        CancellationToken cancellationToken)
    {
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out _))
        {
            return ApiResponse<GetMercadoPagoPaymentStatusResponse>.Fail("Usuario no autenticado.");
        }

        var payment = await dbContext.Payments
            .AsNoTracking()
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Patient)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return ApiResponse<GetMercadoPagoPaymentStatusResponse>.Fail("Pago no encontrado.");
        }

        if (payment.Method != PaymentMethod.MercadoPago)
        {
            return ApiResponse<GetMercadoPagoPaymentStatusResponse>.Fail("El pago no es de Mercado Pago.");
        }

        return ApiResponse<GetMercadoPagoPaymentStatusResponse>.Ok(
            new GetMercadoPagoPaymentStatusResponse(
                payment.Id,
                payment.AppointmentId,
                payment.Amount,
                payment.Status.ToString(),
                payment.PreferenceId,
                payment.MercadoPagoPaymentId,
                payment.CreatedAt,
                payment.UpdatedAt));
    }
}