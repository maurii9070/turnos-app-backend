namespace Turnos.Api.Features.Admins.CreateAdmin;

public record CreateAdminRequest(
    string Dni,
    string FirstName,
    string LastName,
    string Password,
    string? Email,
    string? Phone
);
