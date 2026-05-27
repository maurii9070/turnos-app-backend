using Microsoft.EntityFrameworkCore;
using Turnos.Api.Common.Responses;
using Turnos.Api.Data;
using Turnos.Api.Entities;

namespace Turnos.Api.Features.Specialties.CreateSpecialty;

public class CreateSpecialtyHandler(TurnosDbContext dbContext)
{
    public async Task<ApiResponse<CreateSpecialtyResponse>> Handle(
        CreateSpecialtyRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Specialties
            .AsNoTracking()
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (exists)
        {
            return ApiResponse<CreateSpecialtyResponse>.Fail("Ya existe una especialidad con ese nombre.");
        }

        var specialty = new Specialty
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        dbContext.Specialties.Add(specialty);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateSpecialtyResponse(specialty.Id, specialty.Name, specialty.Description);
        return ApiResponse<CreateSpecialtyResponse>.Ok(response, "Especialidad creada correctamente.");
    }
}
