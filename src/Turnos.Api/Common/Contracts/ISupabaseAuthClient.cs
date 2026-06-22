using Turnos.Api.Common.Security;

namespace Turnos.Api.Common.Contracts;

public interface ISupabaseAuthClient
{
    Task<SupabaseUserInfo?> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default);
}
