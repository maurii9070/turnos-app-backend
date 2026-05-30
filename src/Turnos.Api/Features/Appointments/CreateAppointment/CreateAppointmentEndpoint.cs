using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Appointments.CreateAppointment;

public class CreateAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/appointments", async (
            CreateAppointmentRequest request,
            ClaimsPrincipal user,
            CreateAppointmentHandler handler,
            IValidator<CreateAppointmentRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(request, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "Paciente no encontrado." => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    "Doctor no encontrado." => Results.NotFound(result),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("CreateAppointment")
        .RequireAuthorization()
        .Produces<ApiResponse<CreateAppointmentResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
