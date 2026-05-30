namespace Turnos.Api.Features.Doctors.ListDoctors;

public record ListDoctorsResponse(
    Guid DoctorId,
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    Guid SpecialtyId,
    string SpecialtyName,
    string LicenseNumber,
    decimal ConsultationPrice
);
