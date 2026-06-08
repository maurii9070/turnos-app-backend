namespace Turnos.Api.Features.Patients.CreatePatient;

public record CreatePatientResponse(
    Guid PatientId,
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    string Role,
    bool MustChangePassword,
    DateTime CreatedAt
);
