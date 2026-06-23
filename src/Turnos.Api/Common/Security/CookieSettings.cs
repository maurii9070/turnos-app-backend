namespace Turnos.Api.Common.Security;

public sealed class CookieSettings
{
    public string SameSite { get; set; } = "Lax";
    public bool Secure { get; set; } = false;

    public SameSiteMode GetSameSiteMode()
    {
        return SameSite.ToLowerInvariant() switch
        {
            "none" => SameSiteMode.None,
            "strict" => SameSiteMode.Strict,
            _ => SameSiteMode.Lax
        };
    }
}
