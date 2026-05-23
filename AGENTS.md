# AGENTS.md — SistemaTurnos Backend

## Stack & Project Boundaries

- **.NET 10** Web API. Single project: `src/Turnos.Api/Turnos.Api.csproj`.
- Solution file uses the new **`.slnx` format** (XML), not the legacy `.sln` format.
- Referenced but not yet wired up: EF Core 10 + Npgsql (PostgreSQL), FluentValidation, EFCore.NamingConventions.
- **No tests, no CI, no Docker** — project is in early scaffolding stage.

## Developer Commands

```bash
# Build / run
dotnet build SistemaTurnos.slnx
dotnet run --project src/Turnos.Api/Turnos.Api.csproj

# Dev server URLs (from launchSettings.json)
# HTTP:  http://localhost:5092
# HTTPS: https://localhost:7269
```

## Notes

- `Program.cs` is minimal (only a root `GET /` endpoint). OpenAPI is mapped only in Development.
- `Turnos.Api.http` still references a `/weatherforecast/` endpoint that does not exist (template leftover).
