# QPhising

This repository contains a production-oriented Clean Architecture foundation for QPhising.

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

## Swagger Quality Gate

Use the Swagger quality gate scripts to validate OpenAPI contract standards in local workflows and CI.

- Linux/macOS:
  - `./scripts/check-swagger-quality.sh`
  - `./scripts/check-swagger-quality.sh http://localhost:5050/swagger/v1/swagger.json`
- Windows:
  - `scripts\check-swagger-quality.bat`
  - `scripts\check-swagger-quality.bat https://localhost:7050/swagger/v1/swagger.json`

The CI workflow `.github/workflows/swagger-quality-gate.yml` runs the same gate against `frontend/openapi/proxy-validation.swagger.json` on pushes and pull requests.

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
