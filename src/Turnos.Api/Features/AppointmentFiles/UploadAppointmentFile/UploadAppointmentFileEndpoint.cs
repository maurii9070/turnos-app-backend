using System.Security.Claims;
using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.AppointmentFiles.UploadAppointmentFile;

public class UploadAppointmentFileEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/appointments/{id:guid}/files", async (
            Guid id,
            UploadAppointmentFileRequest request,
            ClaimsPrincipal user,
            UploadAppointmentFileHandler handler,
            IValidator<UploadAppointmentFileRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(id, request, user, ct);

            if (!result.Success)
            {
                return result.Message switch
                {
                    "Turno no encontrado." => Results.NotFound(result),
                    "No tiene permiso para subir archivos a este turno." => Results.Json(result, statusCode: StatusCodes.Status403Forbidden),
                    _ => Results.Conflict(result)
                };
            }

            return Results.Ok(result);
        })
        .WithName("UploadAppointmentFile")
        .RequireAuthorization()
        .Produces<ApiResponse<UploadAppointmentFileResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
