using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Appointments.ConfirmAppointment;

public class ConfirmAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("api/appointments/{id:guid}/confirm", async (
            Guid id,
            ConfirmAppointmentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "Turno no encontrado." => Results.NotFound(result),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("ConfirmAppointment")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.SuperAdmin)))
        .Produces<ApiResponse<ConfirmAppointmentResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
