using System.Security.Claims;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.AppointmentFiles.DeleteAppointmentFile;

public class DeleteAppointmentFileEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/appointments/{appointmentId:guid}/files/{fileId:guid}", async (
            Guid appointmentId,
            Guid fileId,
            ClaimsPrincipal user,
            DeleteAppointmentFileHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(appointmentId, fileId, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "Archivo no encontrado." => Results.NotFound(result),
                    "No tiene permiso para eliminar este archivo." =>
                        Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("DeleteAppointmentFile")
        .RequireAuthorization()
        .Produces<ApiResponse<DeleteAppointmentFileResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
