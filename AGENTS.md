# AGENTS.md — SistemaTurnos Backend

## Stack & Project Boundaries

- **.NET 10** Web API. Single project: `src/Turnos.Api/Turnos.Api.csproj`.
- Solution file uses the new **`.slnx` format** (XML), not the legacy `.sln` format.
- **PostgreSQL 16+** via EF Core 10 + Npgsql. `EFCore.NamingConventions` enforces **snake_case** globally for all tables/columns.
- **FluentValidation** registered via `AddValidatorsFromAssemblyContaining<Program>()`.
- **No tests, no CI, no Docker** — project is in early scaffolding stage.

## Developer Commands

```bash
# Build / run
dotnet build SistemaTurnos.slnx
dotnet run --project src/Turnos.Api/Turnos.Api.csproj

# EF Core migrations (from repo root)
dotnet ef migrations add <Name> --project src/Turnos.Api --startup-project src/Turnos.Api
dotnet ef database update --project src/Turnos.Api --startup-project src/Turnos.Api

# Dev server URLs (from launchSettings.json)
# HTTP:  http://localhost:5092
# HTTPS: https://localhost:7269
```

## Architecture

**Vertical Slice with Feature Folders** — each feature is self-contained under `Features/<Domain>/<Action>/`:

```
Features/
  Specialties/
    CreateSpecialty/
      CreateSpecialtyEndpoint.cs   ← static Map() method, implements IEndpoint
      CreateSpecialtyHandler.cs    ← business logic, DI-injected
      CreateSpecialtyRequest.cs     ← public record (input)
      CreateSpecialtyResponse.cs    ← public record (output)
      CreateSpecialtyValidator.cs  ← FluentValidation.AbstractValidator<T>
```

**Endpoint Discovery**: `app.MapEndpoints()` in `Program.cs` scans the assembly for classes implementing `IEndpoint` (static `Map(IEndpointRouteBuilder)` method) and registers them automatically. No manual `MapXxx()` calls needed in `Program.cs`.

**Handlers must be registered in DI** via `AddTurnosHandlers()` in `ServiceCollectionExtensions.cs`. There is no auto-discovery for handlers yet.

## Database Conventions

- All tables/columns use **snake_case** via `.UseSnakeCaseNamingConvention()` in `DbContext` setup.
- Enums stored as **strings** (not ints) for readability in DB via `.HasConversion<string>()`.
- Audit fields: `IsActive` (soft delete), `CreatedAt`, `UpdatedAt` on all entities except `AppointmentFile`.
- `DateOnly` → PostgreSQL `date`; `TimeOnly` → PostgreSQL `time without time zone`.
- `decimal` money fields use `HasPrecision(18, 2)`.

## Project Structure

```
src/Turnos.Api/
  Common/
    Contracts/     ← Reusable interfaces (IEndpoint, IPasswordHasher, ITokenService)
    Responses/     ← ApiResponse<T>
    Security/       ← Auth implementations (BCryptPasswordHasher, JwtTokenService, JwtSettings)
    Extensions/     ← Extension methods (AuthExtensions, DatabaseExtensions, etc.)
    Helpers/         ← Reusable utilities (DoctorAuthorization)
  Data/            ← TurnosDbContext
  Entities/        ← Domain entities + Enums/
  Features/        ← Vertical slices (Endpoint, Handler, Request, Response, Validator)
  Migrations/      ← EF Core migration files
```

### Extension Methods (Service Setup)

All service configuration uses extension methods in `Common/Extensions/`:

| Method | Purpose |
|--------|---------|
| `AddTurnosDbContext()` | EF Core + PostgreSQL + snake_case |
| `AddTurnosAuth()` | JWT settings, JWT auth, authorization, password hasher, token service |
| `AddTurnosRateLimiting()` | Rate limiting (5 login attempts/min) |
| `AddTurnosHandlers()` | All feature handlers |

**Middleware pipeline** (in `Program.cs`): HTTPS redirect → Rate limiter → Auth → Authorization → MapEndpoints.

**Handlers are registered via `AddTurnosHandlers()`** in `ServiceCollectionExtensions.cs`, not in `Program.cs`.

## Configuration

- `appsettings.Development.json` contains `ConnectionStrings:DefaultConnection` pointing to local PostgreSQL.
- OpenAPI mapped **only in Development** (`app.MapOpenApi()`).
- `Turnos.Api.http` contains test requests for Auth, Specialties, Doctors, Schedules, and Availabilities.

## Important Constraints

- **Auth is manual** — no ASP.NET Identity. `UserRole` enum: `Patient`, `Doctor`, `Admin`, `SuperAdmin`.
- **No tracking by default** is recommended for read queries (`AsNoTracking()`). Writes require explicit `Update()` or `.AsTracking()`.
- All user-facing messages are in **Spanish**, code in **English**.
- **Authorization**: endpoints protegidos usan `.RequireAuthorization()` (cualquier autenticado). Cuando el endpoint debe restringirse a SuperAdmin o al doctor propietario del recurso, se usa `DoctorAuthorization.CanManageDoctorAsync()` en el handler — no hay custom policies.
