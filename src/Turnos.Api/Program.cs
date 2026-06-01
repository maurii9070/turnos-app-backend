using FluentValidation;
using Turnos.Api.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTurnosDbContext(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddTurnosAuth(builder.Configuration);
builder.Services.AddTurnosRateLimiting();
builder.Services.AddTurnosCors(builder.Configuration);
builder.Services.AddTurnosHandlers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();