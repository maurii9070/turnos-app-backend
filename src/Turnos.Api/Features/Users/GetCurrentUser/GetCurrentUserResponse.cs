namespace Turnos.Api.Features.Users.GetCurrentUser;

public record GetCurrentUserResponse(
    Guid UserId,
    string Dni,
    string? Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool MustChangePassword,
    Guid? PatientId,
    DateOnly? DateOfBirth,
    Guid? DoctorId
);
