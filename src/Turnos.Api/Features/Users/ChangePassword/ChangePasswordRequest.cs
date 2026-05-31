namespace Turnos.Api.Features.Users.ChangePassword;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
