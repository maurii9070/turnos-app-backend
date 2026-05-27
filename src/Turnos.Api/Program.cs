using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Infrastructure;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Features.Auth.RegisterPatient;
using Turnos.Api.Features.Specialties.CreateSpecialty;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TurnosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

// Register FluentValidation validators from the current assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register common infrastructure services
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

// Register feature handlers
builder.Services.AddScoped<CreateSpecialtyHandler>();
builder.Services.AddScoped<RegisterPatientHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map all endpoints implementing IEndpoint
app.MapEndpoints();

app.MapGet("/", () => "Hello World! Welcome to Turnos API.");

app.Run();
