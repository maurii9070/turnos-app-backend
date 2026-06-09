using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Settings;

namespace Turnos.Api.Common.Services;

public class MercadoPagoService : IMercadoPagoService
{
    private readonly HttpClient _httpClient;
    private readonly MercadoPagoSettings _settings;
    private readonly ILogger<MercadoPagoService> _logger;

    public MercadoPagoService(HttpClient httpClient, IOptions<MercadoPagoSettings> settings, ILogger<MercadoPagoService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<CreatePreferenceResult> CreatePreferenceAsync(
        Guid paymentId,
        decimal amount,
        string description,
        string payerEmail,
        string payerFirstName,
        string payerLastName,
        CancellationToken ct)
    {
        var isLocalFrontend = _settings.FrontendBaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                              _settings.FrontendBaseUrl.Contains("127.0.0.1");

        var notificationUrl = $"{_settings.WebhookBaseUrl}api/payments/mercadopago/webhook";

        var body = new Dictionary<string, object?>
        {
            ["items"] = new[]
            {
                new
                {
                    title = description,
                    quantity = 1,
                    unit_price = amount,
                    currency_id = "ARS"
                }
            },
            ["external_reference"] = paymentId.ToString(),
            ["notification_url"] = notificationUrl
        };

        if (!string.IsNullOrWhiteSpace(payerEmail) || !string.IsNullOrWhiteSpace(payerFirstName) || !string.IsNullOrWhiteSpace(payerLastName))
        {
            body["payer"] = new
            {
                email = payerEmail,
                name = payerFirstName,
                surname = payerLastName
            };
        }

        if (!isLocalFrontend)
        {
            body["back_urls"] = new
            {
                success = $"{_settings.FrontendBaseUrl}/appointments?status=success",
                failure = $"{_settings.FrontendBaseUrl}/appointments?status=failure",
                pending = $"{_settings.FrontendBaseUrl}/appointments?status=pending"
            };
            body["auto_return"] = "approved";
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "checkout/preferences")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("Authorization", $"Bearer {_settings.AccessToken}");

        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "Mercado Pago devolvió {StatusCode}. Body: {ErrorBody}",
                (int)response.StatusCode, errorBody);
            response.EnsureSuccessStatusCode();
        }

        var json = await response.Content.ReadAsStringAsync();

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        var preferenceId = root.GetProperty("id").GetString()!;
        var initPoint = _settings.Sandbox
            ? root.GetProperty("sandbox_init_point").GetString()!
            : root.GetProperty("init_point").GetString()!;

        return new CreatePreferenceResult(preferenceId, initPoint);
    }

    public async Task<MercadoPagoPaymentInfo?> GetPaymentAsync(long mercadoPagoPaymentId, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"v1/payments/{mercadoPagoPaymentId}");
        request.Headers.Add("Authorization", $"Bearer {_settings.AccessToken}");

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return ParsePaymentInfo(json);
    }

    public async Task<MercadoPagoPaymentInfo?> SearchPaymentByExternalReferenceAsync(string externalReference, CancellationToken ct)
    {
        var url = $"v1/payments/search?external_reference={Uri.EscapeDataString(externalReference)}&limit=1";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {_settings.AccessToken}");

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        var results = root.GetProperty("results");
        if (results.GetArrayLength() == 0)
        {
            return null;
        }

        return ParsePaymentInfo(results[0].GetRawText());
    }

    private static MercadoPagoPaymentInfo? ParsePaymentInfo(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new MercadoPagoPaymentInfo(
            root.GetProperty("id").GetInt64(),
            root.GetProperty("status").GetString()!,
            root.GetProperty("status_detail").GetString()!,
            root.TryGetProperty("external_reference", out var extRef) ? extRef.GetString() : null
        );
    }

    public bool ValidateWebhookSignature(string xSignature, string xRequestId, string dataId)
    {
        var parts = xSignature.Split(',');
        string? ts = null;
        string? hash = null;

        foreach (var part in parts)
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length != 2) continue;
            var key = keyValue[0].Trim();
            var value = keyValue[1].Trim();
            if (key == "ts") ts = value;
            else if (key == "v1") hash = value;
        }

        if (ts is null || hash is null) return false;

        var manifest = !string.IsNullOrEmpty(dataId) && !string.IsNullOrEmpty(xRequestId)
            ? $"id:{dataId.ToLowerInvariant()};request-id:{xRequestId};ts:{ts};"
            : !string.IsNullOrEmpty(dataId)
                ? $"id:{dataId.ToLowerInvariant()};ts:{ts};"
                : $"ts:{ts};";

        var computed = Convert.ToHexString(
            HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(_settings.WebhookSecret),
                Encoding.UTF8.GetBytes(manifest)
            )
        ).ToLowerInvariant();

        return computed == hash.ToLowerInvariant();
    }
}