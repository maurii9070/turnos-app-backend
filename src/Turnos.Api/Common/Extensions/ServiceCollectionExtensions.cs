using Turnos.Api.Features.Auth.Login;
using Turnos.Api.Features.Auth.Logout;
using Turnos.Api.Features.Auth.RefreshToken;
using Turnos.Api.Features.Auth.RegisterPatient;
using Turnos.Api.Features.Doctors.Availabilities.CreateAvailability;
using Turnos.Api.Features.Doctors.Availabilities.DeleteAvailability;
using Turnos.Api.Features.Doctors.Availabilities.ListAvailabilities;
using Turnos.Api.Features.Doctors.Availabilities.UpdateAvailability;
using Turnos.Api.Features.Doctors.CreateDoctor;
using Turnos.Api.Features.Doctors.DeactivateDoctor;
using Turnos.Api.Features.Doctors.GetDoctor;
using Turnos.Api.Features.Doctors.ListDoctors;
using Turnos.Api.Features.Doctors.Schedules.GetSchedules;
using Turnos.Api.Features.Doctors.Schedules.SetSchedules;
using Turnos.Api.Features.Doctors.UpdateDoctor;
using Turnos.Api.Features.Specialties.CreateSpecialty;
using Turnos.Api.Features.Specialties.DeleteSpecialty;
using Turnos.Api.Features.Specialties.GetSpecialties;
using Turnos.Api.Features.Specialties.GetSpecialtyById;
using Turnos.Api.Features.Specialties.UpdateSpecialty;

namespace Turnos.Api.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTurnosHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreateSpecialtyHandler>();
        services.AddScoped<GetSpecialtiesHandler>();
        services.AddScoped<GetSpecialtyByIdHandler>();
        services.AddScoped<UpdateSpecialtyHandler>();
        services.AddScoped<DeleteSpecialtyHandler>();

        services.AddScoped<RegisterPatientHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();

        services.AddScoped<CreateDoctorHandler>();
        services.AddScoped<ListDoctorsHandler>();
        services.AddScoped<GetDoctorHandler>();
        services.AddScoped<UpdateDoctorHandler>();
        services.AddScoped<DeactivateDoctorHandler>();

        services.AddScoped<GetSchedulesHandler>();
        services.AddScoped<SetSchedulesHandler>();
        services.AddScoped<ListAvailabilitiesHandler>();
        services.AddScoped<CreateAvailabilityHandler>();
        services.AddScoped<UpdateAvailabilityHandler>();
        services.AddScoped<DeleteAvailabilityHandler>();

        return services;
    }
}
