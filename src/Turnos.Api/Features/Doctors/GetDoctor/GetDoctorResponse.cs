namespace Turnos.Api.Features.Doctors.GetDoctor;

public record DoctorScheduleResponse(Guid Id, string DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

public record DoctorAvailabilityResponse(Guid Id, string Date, TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable);

public record SpecialtyInfo(Guid Id, string Name);

public record GetDoctorResponse(
    Guid DoctorId,
    Guid UserId,
    string Dni,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    SpecialtyInfo Specialty,
    string LicenseNumber,
    decimal ConsultationPrice,
    List<DoctorScheduleResponse> Schedules,
    List<DoctorAvailabilityResponse> Availabilities
);
