namespace Turnos.Api.Features.Auth.Login;

public record LoginResponse(string AccessToken, string RefreshToken, string Role);
