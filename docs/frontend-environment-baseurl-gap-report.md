# Frontend Environment and Base URL Configuration Gap Report

Last updated: 2026-03-28  
Owner: Codex  
Scope: `frontend/**`, runtime compose wiring in `docker-compose.yml`

## Audit objective

Verify that frontend API/gateway base URLs are centrally configured per environment (development, staging, production) and not hardcoded in components/services.

## Audit method

1. Inspected Angular workspace/build configuration for environment file replacement or runtime config hooks.
2. Searched frontend source for HTTP transport usage and hardcoded URL patterns.
3. Cross-checked Docker/runtime composition for frontend-to-gateway connectivity configuration.

## Findings summary

- No hardcoded API URL usage was found in frontend source, but this is because no API transport layer currently exists yet.
- Angular environment configuration scaffolding is missing (no `src/environments/*`, no build-time file replacements).
- Runtime-injected frontend config is missing (no `/assets/config.json` pattern and no config bootstrap service).
- Docker compose defines gateway and frontend services, but frontend has no configurable gateway base URL input at runtime/build time.

## Evidence

| Evidence area | Result |
|---|---|
| Angular build configuration (`frontend/angular.json`) | Only `production` budget/output hash config exists; no `fileReplacements`, no environment targets for API base URL values. |
| Frontend transport/url scan (`frontend/src/**`) | No `HttpClient`, `fetch`, `http://`, `https://`, `/api`, or `baseUrl` references detected. |
| Frontend runtime bootstrap (`frontend/src/main.ts`, `frontend/src/app/app.module.ts`) | No configuration provider/initializer to load runtime API config before app startup. |
| Container runtime (`docker-compose.yml`, `frontend/Dockerfile`) | Gateway service exists and frontend depends on it, but frontend image/startup has no injected API base URL env key consumption. |

## Configuration gap matrix

| Gap ID | Missing key / concern | Current state | Required target source | Environment impact |
|---|---|---|---|---|
| CFG-01 | `apiBaseUrl` | Missing | Frontend environment/runtime config (`gateway` entrypoint URL, e.g. `http://localhost:5001` for local) | Dev/Staging/Prod |
| CFG-02 | `gatewayBaseUrl` (if separated from API prefix) | Missing | Frontend environment/runtime config | Dev/Staging/Prod |
| CFG-03 | `apiPathPrefix` (e.g., `/api`) | Missing | Frontend environment/runtime config (or constant under config service) | Dev/Staging/Prod |
| CFG-04 | Environment-specific config source selection | Missing | Angular build `fileReplacements` and/or runtime `assets/config.*.json` strategy | Dev/Staging/Prod |
| CFG-05 | Startup validation for required config keys | Missing | Config bootstrap service (`APP_INITIALIZER`) that fails fast when required keys absent | Dev/Staging/Prod |
| CFG-06 | Docker/frontend runtime injection contract | Missing | Compose + deployment manifest env-to-runtime mapping (e.g., mounted `config.json` template or entrypoint substitution) | Staging/Prod |

## Recommended centralized key set (authoritative)

Use one authoritative frontend config shape consumed by a single config service:

- `gatewayBaseUrl` (required)
- `apiBaseUrl` (required; can be derived from gateway + prefix)
- `apiPathPrefix` (required; default `/api`)
- `requestTimeoutMs` (optional but recommended)

## Target per-environment source mapping

| Environment | Recommended source | Notes |
|---|---|---|
| Development | `src/environments/environment.ts` (or runtime local config file) | Must point to local gateway host/port used by frontend dev server/browser. |
| Staging | Runtime-injected config artifact (e.g., `/assets/config.json`) | Prefer immutable frontend image with deployment-time config override. |
| Production | Runtime-injected config artifact (e.g., `/assets/config.json`) | Avoid rebuilding image for URL-only changes; enforce startup validation. |

## Exit criteria for this gap

1. Frontend has one centralized config service and no feature-level hardcoded URL literals.
2. `apiBaseUrl`/`gatewayBaseUrl` are provided for each environment path.
3. Startup fails fast (clear error) if required config keys are missing.
4. All future API integrations (including generated OpenAPI clients) read base URL from centralized config only.
