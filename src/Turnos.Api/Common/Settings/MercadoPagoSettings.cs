namespace Turnos.Api.Common.Settings;

public class MercadoPagoSettings
{
    public string AccessToken { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string FrontendBaseUrl { get; set; } = string.Empty;
    public string WebhookBaseUrl { get; set; } = string.Empty;
    public bool Sandbox { get; set; } = true;
}