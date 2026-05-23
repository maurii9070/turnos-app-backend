using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common;
using Turnos.Api.Data;
using Turnos.Api.Features.Specialties.CreateSpecialty;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TurnosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

// Register FluentValidation validators from the current assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register feature handlers (pilot: CreateSpecialty)
builder.Services.AddScoped<CreateSpecialtyHandler>();

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
