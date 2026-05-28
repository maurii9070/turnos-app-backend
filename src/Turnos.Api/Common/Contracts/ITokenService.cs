using Turnos.Api.Entities;

namespace Turnos.Api.Common.Contracts;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
