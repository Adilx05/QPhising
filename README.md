# QPhising

QPhising is an enterprise-grade **Web Page Tracking & Visitor Analytics** platform built with a backend-first Clean Architecture approach.

It provides:

- secure campaign + tracking-page lifecycle management,
- public tracking landing resolution (`/p/{slug}`),
- visit ingestion with privacy-aware IP handling,
- analytics dashboards (totals, uniques, trends, top pages, recent visits),
- setup/runtime configuration flow,
- API contract-driven frontend integration through generated proxies.

---

## Repository Layout

- `backend/`
  - `Domain/` business rules and aggregates
  - `Application/` CQRS handlers, contracts, validators
  - `Infrastructure/` EF Core persistence, external integrations
  - `API/` HTTP endpoints + Swagger + auth + rate limiting
  - `Gateway/` Ocelot routing and edge concerns
- `frontend/`
  - Angular UI (PrimeNG + Tailwind)
  - generated API proxies under `src/app/shared/proxy`
- `docs/`
  - architecture references, API contracts, operational runbooks
- `scripts/`
  - quality gates, proxy generation/validation, smoke checks
- `deploy/env/`
  - environment templates for local/staging/production

---

## Core Capabilities

### Tracking & Campaigns

- Tracking page CRUD and lifecycle controls
- Campaign-to-tracking-page linkage
- Public landing via slug routes (`/p/{slug}`)
- Campaign-state-aware public availability (non-active campaigns return 404)

### Visit Collection

- Public ingestion endpoints for tracking events
- Deduplication/throttling protections
- Optional bot filtering
- UTM/referrer/device-aware event capture

### Analytics

- Total visits
- Unique visits (session/fingerprint/IP fallback strategy)
- Top pages ranking
- Time-based trends (hour/day/week)
- Recent visit stream for operator visibility

### Security & Reliability

- JWT-based authentication + role-based authorization (`Admin`, `Operator`, `Viewer`)
- Public endpoint rate limiting
- ProblemDetails-based API errors
- Correlation ID propagation (`X-Correlation-Id`)
- Structured JSON logging
- Soft-delete semantics for core entities

---

## Tech Stack

### Backend

- .NET (Clean Architecture)
- MediatR (CQRS)
- FluentValidation
- AutoMapper
- EF Core + PostgreSQL
- Ocelot Gateway

### Frontend

- Angular
- PrimeNG
- TailwindCSS
- Generated TypeScript API clients (`openapi-typescript-codegen`)

### DevOps

- Docker + Docker Compose
- GitHub Actions (CI + release baseline)

---

## Local Development (Windows-native first, container optional)

> The project is optimized for local Windows-native multi-startup (API + Gateway), while containerized runtime is available for full-stack orchestration.

### 1) Configure runtime settings

Create runtime overlays from templates when needed:

- `backend/API/appsettings.runtime.json.example`
- `backend/Gateway/appsettings.runtime.json.example`
- `deploy/env/.env.local.example`

### 2) Run API + Gateway locally

- API defaults
  - HTTP: `http://localhost:5050`
  - HTTPS: `https://localhost:7050`
- Gateway defaults
  - HTTP: `http://localhost:8080`
  - HTTPS: `https://localhost:8443`

Swagger is available in development mode (or when explicitly enabled by feature flag).

### 3) Frontend

Run the Angular app from `frontend/` and ensure gateway/API endpoints are reachable for generated proxy calls.

---

## Containerized Runtime

Use root `docker-compose.yml`.

### Standard stack

```bash
cp deploy/env/.env.local.example .env
docker compose up --build
```

### With Redis profile enabled

```bash
docker compose --profile redis up --build
```

### Exposed services

- API: `http://localhost:5050`
- Gateway: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
- Redis (optional): `localhost:6379`

---

## Database & Migrations

EF Core migrations are the source of truth for schema lifecycle.

### Configure connection

Set `ConnectionStrings:DefaultConnection` in runtime settings or environment variables.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=qphising;Username=qphising;Password=change-me"
  }
}
```

### Common commands (from `backend/API`)

```bash
dotnet ef migrations add <MigrationName> --output-dir Infrastructure/Persistence/Migrations
dotnet ef database update
dotnet ef migrations list
```

### Notes

- `POST /api/setup/test-db` validates connectivity only.
- Schema changes must be delivered via migrations, not ad-hoc runtime DDL.

---

## API Contract, Swagger, and Proxy Workflow

Backend contracts are authoritative.

When API contracts change:

1. Update backend domain/application/API.
2. Ensure Swagger reflects the new contract.
3. Regenerate frontend proxies.
4. Fix frontend compile/runtime usage against generated clients.

### Proxy generation

- Linux/macOS:
  - `./scripts/generate-proxy.sh`
- Windows:
  - `scripts\generate-proxy.bat`

### Proxy determinism validation

- Linux/macOS: `./scripts/validate-proxy-generation.sh`
- Windows: `scripts\validate-proxy-generation.bat`

---

## Quality Gates

### Swagger quality gate

- Linux/macOS: `./scripts/check-swagger-quality.sh`
- Windows: `scripts\check-swagger-quality.bat`

### Gateway ↔ Swagger alignment

- Linux/macOS: `./scripts/check-gateway-swagger-alignment.sh`
- Windows: `scripts\check-gateway-swagger-alignment.bat`

### Proxy ↔ Gateway consistency

- Linux/macOS: `./scripts/check-proxy-gateway-consistency.sh`
- Windows: `scripts\check-proxy-gateway-consistency.bat`

### Frontend UI smoke checks

Use the smoke checker scripts in `scripts/` to verify dashboard/tracking/campaign core views are present and wired.

---

## CI/CD

- `.github/workflows/ci.yml`
  - backend restore/build/test
  - frontend install/build
  - contract/proxy/gateway checks
- `.github/workflows/release.yml`
  - environment-scoped release gating
  - semantic version input checks
  - pre-release build/test verification

---

## Operational Notes

- Setup wizard governs first-run bootstrap and runtime configuration persistence.
- Redis is optional unless explicitly required by a feature/profile.
- Main management and analytics endpoints are protected; public tracking routes are intentionally constrained and rate-limited.
- Correlation IDs are propagated API ↔ Gateway for traceability.

---

## Documentation Index

For deeper implementation details, start with:

- `docs/architecture/`
- `docs/api/`
- `docs/operations/`

---

## License

Internal project repository. Add/adjust license policy here if the distribution model changes.
