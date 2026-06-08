namespace Turnos.Api.Features.Patients.ListPatients;

public record ListPatientsResponse(
    Guid PatientId,
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? DateOfBirth,
    bool IsActive,
    int TotalAppointments,
    DateTime CreatedAt
);
