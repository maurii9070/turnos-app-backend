namespace Turnos.Api.Features.Admins.CreateAdmin;

public record CreateAdminResponse(
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    string Role,
    bool MustChangePassword,
    DateTime CreatedAt
);
