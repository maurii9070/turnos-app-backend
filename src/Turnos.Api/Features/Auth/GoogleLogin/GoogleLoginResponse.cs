namespace Turnos.Api.Features.Auth.GoogleLogin;

public record GoogleLoginResponse(string AccessToken, string RefreshToken, string Role);
