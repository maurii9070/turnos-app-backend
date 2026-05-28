using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Auth.RefreshToken;

public record RefreshTokenResponse(string AccessToken, string Role);
