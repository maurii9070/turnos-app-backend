namespace Turnos.Api.Features.Patients.GetPatientByDni;

public record GetPatientByDniResponse(
    Guid PatientId,
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateOnly? DateOfBirth,
    bool IsActive,
    int TotalAppointments,
    DateTime CreatedAt
);
