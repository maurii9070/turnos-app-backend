namespace Turnos.Api.Features.Doctors.Appointments.GetDoctorAppointments;

public record GetDoctorAppointmentsResponse(Guid Id, string Date, string StartTime, string Status);
