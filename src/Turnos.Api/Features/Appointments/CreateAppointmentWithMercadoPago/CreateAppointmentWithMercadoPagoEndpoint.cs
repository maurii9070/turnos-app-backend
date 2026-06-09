using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Appointments.CreateAppointmentWithMercadoPago;

public class CreateAppointmentWithMercadoPagoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/appointments/mercadopago", async (
            CreateAppointmentWithMercadoPagoRequest request,
            ClaimsPrincipal user,
            CreateAppointmentWithMercadoPagoHandler handler,
            IValidator<CreateAppointmentWithMercadoPagoRequest> validator,
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
                    "Usuario no autenticado." => Results.Json(result, statusCode: StatusCodes.Status401Unauthorized),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("CreateAppointmentWithMercadoPago")
        .RequireAuthorization()
        .Produces<ApiResponse<CreateAppointmentWithMercadoPagoResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}