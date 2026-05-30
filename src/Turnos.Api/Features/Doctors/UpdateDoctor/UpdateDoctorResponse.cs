namespace Turnos.Api.Features.Doctors.UpdateDoctor;

public record UpdateDoctorResponse(
    Guid DoctorId,
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    string SpecialtyName,
    string LicenseNumber,
    decimal ConsultationPrice
);
