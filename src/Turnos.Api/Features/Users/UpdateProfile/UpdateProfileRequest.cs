namespace Turnos.Api.Features.Users.UpdateProfile;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? DateOfBirth
);
