namespace Turnos.Api.Features.Doctors.CreateDoctor;

public record CreateDoctorRequest(
    string Dni,
    string FirstName,
    string LastName,
    string Password,
    Guid SpecialtyId,
    string LicenseNumber,
    decimal ConsultationPrice,
    string? Email,
    string? Phone
);
