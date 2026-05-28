namespace Turnos.Api.Features.Specialties.GetSpecialtyById;

public record GetSpecialtyByIdResponse(Guid Id, string Name, string? Description);
