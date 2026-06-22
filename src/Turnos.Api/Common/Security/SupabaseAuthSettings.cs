namespace Turnos.Api.Common.Security;

public sealed class SupabaseAuthSettings
{
    public string Url { get; set; } = null!;
    public string AnonKey { get; set; } = null!;
}
