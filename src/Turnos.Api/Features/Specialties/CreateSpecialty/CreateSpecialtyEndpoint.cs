using FluentValidation;
using Turnos.Api.Common;

namespace Turnos.Api.Features.Specialties.CreateSpecialty;

public class CreateSpecialtyEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("api/specialties", async (
            CreateSpecialtyRequest request,
            CreateSpecialtyHandler handler,
            IValidator<CreateSpecialtyRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var result = await handler.Handle(request, ct);
            return result.Success
                ? Results.Ok(result)
                : Results.Conflict(result);
        })
        .WithName("CreateSpecialty")
        .Produces<ApiResponse<CreateSpecialtyResponse>>(StatusCodes.Status200OK)
        .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);
    }
}
