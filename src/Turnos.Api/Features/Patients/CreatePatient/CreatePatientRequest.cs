namespace Turnos.Api.Features.Patients.CreatePatient;

public record CreatePatientRequest(
    string Dni,
    string FirstName,
    string LastName,
    string Password,
    DateOnly DateOfBirth,
    string? Phone,
    string? Email
);
