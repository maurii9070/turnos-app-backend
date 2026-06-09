using Turnos.Api.Common.Settings;

namespace Turnos.Api.Common.Contracts;

public interface IMercadoPagoService
{
    Task<CreatePreferenceResult> CreatePreferenceAsync(
        Guid paymentId,
        decimal amount,
        string description,
        string payerEmail,
        string payerFirstName,
        string payerLastName,
        CancellationToken ct);

    Task<MercadoPagoPaymentInfo?> GetPaymentAsync(long mercadoPagoPaymentId, CancellationToken ct);

    Task<MercadoPagoPaymentInfo?> SearchPaymentByExternalReferenceAsync(string externalReference, CancellationToken ct);

    bool ValidateWebhookSignature(string xSignature, string xRequestId, string dataId);
}

public record CreatePreferenceResult(
    string PreferenceId,
    string InitPoint
);

public record MercadoPagoPaymentInfo(
    long Id,
    string Status,
    string StatusDetail,
    string? ExternalReference
);