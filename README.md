# QPhising

This repository contains a production-oriented Clean Architecture foundation for QPhising's web page tracking and visitor analytics platform.

## Structure

- `backend/` - Domain/Application/API/Gateway runnable services.
- `frontend/` - Angular application and generated API client location.
- `docs/` - Architecture decisions and operational documentation.
- `scripts/` - Development and helper scripts.

## Configuration Model

Runnable backend projects (`backend/API`, `backend/Gateway`) use the same layered configuration model:

1. `appsettings.json` (base defaults)
2. `appsettings.{Environment}.json` (environment override)
3. `appsettings.runtime.json` (optional setup-wizard runtime overlay)

`appsettings.runtime.json` is intentionally gitignored and should be created by setup/runtime provisioning. Example templates:

- `backend/API/appsettings.runtime.json.example`
- `backend/Gateway/appsettings.runtime.json.example`

## Local Startup

### API

- HTTP: `http://localhost:5050`
- HTTPS: `https://localhost:7050`
- Swagger: enabled in Development or when `FeatureFlags:SwaggerEnabled=true`

### Gateway

- HTTP: `http://localhost:8080`
- HTTPS: `https://localhost:8443`
- Routes in `backend/Gateway/ocelot*.json`

### Visual Studio Multi-Startup

Set multiple startup projects:

1. `QPhising.Api`
2. `QPhising.Gateway`

This starts API and Gateway together with non-conflicting ports.

## Database Bootstrap & EF Core Migrations

The API now uses EF Core with Npgsql for schema lifecycle management (`UseNpgsql(connectionString)`) and applies migrations on startup in Development by default.

### 1) Configure local PostgreSQL connection

Set `ConnectionStrings:DefaultConnection` in:

- `backend/API/appsettings.runtime.json` (recommended for local overrides), or
- environment variable `ConnectionStrings__DefaultConnection`.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=change-me"
  }
}
```

### 2) Startup migration behavior

Database settings (under `Database`):

- `ApplyMigrationsOnStartup`: force migrations during startup (default `true` in Development, `false` otherwise).
- `ContinueOnMigrationFailure`: continue startup after migration failure (`false` recommended for fail-fast behavior).

The API logs migration attempts and failures with structured log entries.

### 3) Run migrations manually (recommended for controlled environments)

From `backend/API`:

- Create a migration:
  - `dotnet ef migrations add <MigrationName> --output-dir Infrastructure/Persistence/Migrations`
- Apply migrations to DB:
  - `dotnet ef database update`
- List migrations:
  - `dotnet ef migrations list`

EF migration history is stored in the default `__EFMigrationsHistory` table.

### 4) Setup wizard `test-db` endpoint scope

`POST /api/setup/test-db` remains a connectivity check only (`CanConnect`) and does **not** create/update schema. Schema lifecycle is managed exclusively via EF Core migrations.

## Swagger Quality Gate

Use the Swagger quality gate scripts to validate OpenAPI contract standards in local workflows and CI.

- Linux/macOS:
  - `./scripts/check-swagger-quality.sh`
  - `./scripts/check-swagger-quality.sh http://localhost:5050/swagger/v1/swagger.json`
- Windows:
  - `scripts\check-swagger-quality.bat`
  - `scripts\check-swagger-quality.bat https://localhost:7050/swagger/v1/swagger.json`

The CI workflow `.github/workflows/swagger-quality-gate.yml` runs the same gate against `frontend/openapi/proxy-validation.swagger.json` on pushes and pull requests.

## Gateway/Swagger Alignment Check

Use gateway alignment scripts to verify that Ocelot API routes are backed by matching downstream Swagger operations.

- Linux/macOS:
  - `./scripts/check-gateway-swagger-alignment.sh`
  - `./scripts/check-gateway-swagger-alignment.sh --swagger frontend/openapi/proxy-validation.swagger.json --ocelot backend/Gateway/ocelot.json`
- Windows:
  - `scripts\check-gateway-swagger-alignment.bat`
  - `scripts\check-gateway-swagger-alignment.bat --swagger frontend\openapi\proxy-validation.swagger.json --ocelot backend\Gateway\ocelot.json`

The check fails when an API route in Ocelot has no matching Swagger path or does not permit all matching Swagger HTTP methods.

## Proxy/Gateway Consistency Check

Use proxy/gateway consistency scripts to verify generated frontend proxy operations are routable through configured Ocelot upstream paths.

- Linux/macOS:
  - `./scripts/check-proxy-gateway-consistency.sh`
  - `./scripts/check-proxy-gateway-consistency.sh --ocelot backend/Gateway/ocelot.json --proxy-dir frontend/src/app/shared/proxy/services`
- Windows:
  - `scripts\check-proxy-gateway-consistency.bat`
  - `scripts\check-proxy-gateway-consistency.bat --ocelot backend\Gateway\ocelot.json --proxy-dir frontend\src\app\shared\proxy\services`

The check fails if a generated service operation path/method pair is not covered by current gateway route templates.

## Proxy Generation

Use repeatable proxy scripts under `scripts/` to regenerate TypeScript API clients from a running backend Swagger endpoint.

- Linux/macOS:
  - `./scripts/generate-proxy.sh`
  - `./scripts/generate-proxy.sh http://localhost:5050/swagger/v1/swagger.json`
- Windows:
  - `scripts\generate-proxy.bat`
  - `scripts\generate-proxy.bat http://localhost:5050/swagger/v1/swagger.json`

Both scripts:

- read OpenAPI/Swagger from the provided URL,
- validate required Swagger metadata and endpoint prerequisites before generation,
- pin `openapi-typescript-codegen` to deterministic version `0.29.0` (override with `GENERATOR_VERSION`),
- regenerate clients into `frontend/src/app/shared/proxy`,
- normalize generated text line endings to LF for deterministic cross-platform diffs.

Use proxy validation scripts to assert the standardized Swagger fixture regenerates the checked-in proxies without drift:

- Linux/macOS: `./scripts/validate-proxy-generation.sh`
- Windows: `scripts\validate-proxy-generation.bat`
