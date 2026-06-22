namespace Turnos.Api.Common.Security;

public sealed record SupabaseUserInfo(
    string Id,
    string? Email,
    bool EmailVerified,
    string? FirstName,
    string? LastName,
    string? AvatarUrl);
