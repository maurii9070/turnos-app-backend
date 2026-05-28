using FluentValidation;
using Turnos.Api.Common.Contracts;
using Turnos.Api.Common.Responses;

namespace Turnos.Api.Features.Auth.RegisterPatient;

public class RegisterPatientEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register-patient", async (
            RegisterPatientRequest request,
            RegisterPatientHandler handler,
            IValidator<RegisterPatientRequest> validator,
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
        .WithName("RegisterPatient")
        .Produces<ApiResponse<RegisterPatientResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}
