using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Payments.MercadoPagoWebhook;

public class MercadoPagoWebhookHandler
{
    private readonly TurnosDbContext _dbContext;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly ILogger<MercadoPagoWebhookHandler> _logger;

    public MercadoPagoWebhookHandler(
        TurnosDbContext dbContext,
        IMercadoPagoService mercadoPagoService,
        ILogger<MercadoPagoWebhookHandler> logger)
    {
        _dbContext = dbContext;
        _mercadoPagoService = mercadoPagoService;
        _logger = logger;
    }

    public async Task<IResult> HandleWithValidationAsync(
        MercadoPagoWebhookNotification notification,
        string xSignature,
        string xRequestId,
        string dataId,
        CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(xSignature))
        {
            if (!_mercadoPagoService.ValidateWebhookSignature(xSignature, xRequestId, dataId))
            {
                _logger.LogWarning("Firma de webhook de Mercado Pago no válida.");
                return Results.Unauthorized();
            }
        }
        else
        {
            _logger.LogWarning("Webhook recibido sin firma x-signature. Se procesa sin validación.");
        }

        try
        {
            await ProcessNotificationAsync(notification, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar webhook de Mercado Pago");
        }

        return Results.Ok(new { status = "ok" });
    }

    private async Task ProcessNotificationAsync(MercadoPagoWebhookNotification notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Webhook de Mercado Pago recibido - Type: {Type}, Action: {Action}, DataId: {DataId}",
            notification.Type, notification.Action, notification.Data?.Id);

        if (notification.Type != "payment")
        {
            _logger.LogInformation("Tipo de notificación ignorado: {Type}", notification.Type);
            return;
        }

        if (notification.Data?.Id is null || !long.TryParse(notification.Data.Id, out var mpPaymentId))
        {
            _logger.LogWarning("Data.Id no válido en la notificación: {DataId}", notification.Data?.Id);
            return;
        }

        var mpPayment = await _mercadoPagoService.GetPaymentAsync(mpPaymentId, ct);
        if (mpPayment is null)
        {
            _logger.LogWarning("No se pudo obtener el pago {MpPaymentId} desde Mercado Pago", mpPaymentId);
            return;
        }

        _logger.LogInformation(
            "Pago de Mercado Pago obtenido - Id: {Id}, Status: {Status}, ExternalReference: {ExternalRef}",
            mpPayment.Id, mpPayment.Status, mpPayment.ExternalReference);

        if (string.IsNullOrEmpty(mpPayment.ExternalReference) ||
            !Guid.TryParse(mpPayment.ExternalReference, out var paymentId))
        {
            _logger.LogWarning("ExternalReference no válido: {ExternalRef}", mpPayment.ExternalReference);
            return;
        }

        var payment = await _dbContext.Payments
            .AsTracking()
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

        if (payment is null)
        {
            _logger.LogWarning("Pago local no encontrado para ExternalReference: {ExternalRef}", mpPayment.ExternalReference);
            return;
        }

        payment.MercadoPagoPaymentId = mpPayment.Id.ToString();
        payment.UpdatedAt = DateTime.UtcNow;

        var newStatus = MapPaymentStatus(mpPayment.Status);

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
                    _logger.LogInformation(
                        "Turno {AppointmentId} confirmado automáticamente por pago aprobado de Mercado Pago",
                        payment.Appointment.Id);
                }
            }

            _logger.LogInformation(
                "Pago {PaymentId} actualizado de {OldStatus} a {NewStatus}",
                payment.Id, payment.Status, newStatus);
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private static PaymentStatus MapPaymentStatus(string mpStatus) => mpStatus switch
    {
        "approved" => PaymentStatus.Approved,
        "pending" or "in_process" or "authorized" => PaymentStatus.Pending,
        _ => PaymentStatus.Rejected
    };
}