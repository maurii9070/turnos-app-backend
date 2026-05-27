namespace Turnos.Api.Features.Auth.RegisterPatient;

public record RegisterPatientResponse(
    Guid Id,
    string Dni,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt);
