# QPhising

This repository has been reset in Phase 0 and now contains a clean, production-oriented skeleton.

## Structure

- `backend/` - Domain/Application/Infrastructure/API layers
- `gateway/` - Edge gateway and cross-cutting middleware scaffolding
- `worker/` - Background processing scaffolding
- `frontend/` - Angular application and generated API client location
- `docs/` - Architecture decisions and operational documentation
- `scripts/` - Development and compose helper scripts

## Next Steps

Follow `TASKS.md` sequentially for implementation phases.

## Proxy Generation (Phase 2)

Use the repeatable proxy scripts under `scripts/` to regenerate TypeScript API clients from a running backend Swagger endpoint.

- Linux/macOS:
  - `./scripts/generate-proxy.sh`
  - `./scripts/generate-proxy.sh http://localhost:5050/swagger/v1/swagger.json`
- Windows:
  - `scripts\generate-proxy.bat`
  - `scripts\generate-proxy.bat http://localhost:5050/swagger/v1/swagger.json`

Both scripts:
- read OpenAPI/Swagger from the provided URL (default: `http://localhost:5000/swagger/v1/swagger.json`),
- regenerate clients into `frontend/src/app/shared/proxy`,
- replace previous generated output for deterministic day-to-day development.
