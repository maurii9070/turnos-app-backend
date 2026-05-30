using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;
using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Features.Doctors.CreateDoctor;

public class CreateDoctorEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/doctors", async (
            CreateDoctorRequest request,
            CreateDoctorHandler handler,
            IValidator<CreateDoctorRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.HandleAsync(request, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.Conflict(result);
        })
        .WithName("CreateDoctor")
        .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.SuperAdmin)))
        .Produces<ApiResponse<CreateDoctorResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
