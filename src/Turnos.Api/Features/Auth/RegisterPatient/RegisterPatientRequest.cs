namespace Turnos.Api.Features.Auth.RegisterPatient;

public record RegisterPatientRequest(
    string Dni,
    string FirstName,
    string LastName,
    string Password,
    DateOnly DateOfBirth);
