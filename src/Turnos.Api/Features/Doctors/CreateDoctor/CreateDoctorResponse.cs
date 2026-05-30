namespace Turnos.Api.Features.Doctors.CreateDoctor;

public record CreateDoctorResponse(
    Guid DoctorId,
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string SpecialtyName,
    string LicenseNumber,
    decimal ConsultationPrice,
    DateTime CreatedAt
);
