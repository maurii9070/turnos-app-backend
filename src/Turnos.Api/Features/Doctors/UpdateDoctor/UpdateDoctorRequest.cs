namespace Turnos.Api.Features.Doctors.UpdateDoctor;

public record UpdateDoctorRequest(
    Guid SpecialtyId,
    string LicenseNumber,
    decimal ConsultationPrice
);
