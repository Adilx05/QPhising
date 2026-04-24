# QPhising

QPhising is an enterprise-grade **Web Page Tracking & Visitor Analytics** platform built with a backend-first Clean Architecture approach.

It provides:

- secure campaign + tracking-page lifecycle management,
- public tracking landing resolution (`/p/{slug}`),
- visit ingestion with privacy-aware IP handling,
- analytics dashboards (totals, uniques, trends, top pages, recent visits),
- health/readiness-first runtime operations (`/health/live`, `/health/ready`),
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

### Campaign Lifecycle Management

- Campaign CRUD and lifecycle transitions (`Draft`, `Active`, `Paused`, terminal states) are managed through authenticated campaign endpoints.
- Campaign-to-tracking-page linkage is persisted, so each campaign can control a concrete public page surface.
- Public slug resolution (`/p/{slug}`) is campaign-state-aware; non-active campaign windows are intentionally served as `404`.
- Campaign detail workflows expose validity window and public-link context for operator-side orchestration.

### Tracking & Visit Collection

- Tracking pages are managed as first-class resources, then linked to campaign workflows where needed.
- Public and slug-based visit ingestion endpoints collect click events without exposing admin contracts.
- Ingestion applies deduplication/rate-limit guardrails and optional bot filtering for cleaner analytics.
- Event capture supports UTM/referrer/user-agent/session/fingerprint context with privacy-aware IP policy handling.

### Template Management

- HTML page templates are versioned and managed through dedicated template endpoints and UI workflows.
- Campaign/tracking create flows can start from template-backed content or a blank page contract.
- Template preview paths are integrated into operator flows so landing content can be validated before publish/lifecycle actions.

### Audit Log Querying

- Queryable audit-log endpoints support filtered access by actor, result, endpoint/action taxonomy, correlation id, and time window.
- Admin/Operator consoles can inspect security-relevant and lifecycle-critical events without direct database access.
- Audit records preserve outcome metadata (status, correlation, timestamp context) for incident review and traceability.

### Analytics & Report Export (CSV/PDF)

- Analytics pipelines provide totals, unique clicks, top pages, and trend windows (hour/day/week) from visit events.
- Export endpoints generate CSV/PDF reports for global or selected tracking-page scope with summary/detailed levels.
- Report payloads are contract-first and proxy-friendly, enabling frontend export workflows without handwritten API duplication.

### Localization (TR/EN) & Theme Support

- App-shell and feature surfaces support bilingual TR/EN experience with contract-aligned terminology.
- Localization coverage includes auth, dashboard, tracking, templates, audit, and reporting flows.
- Theme toggle support (including dark mode) is available from top-level navigation to keep operator UX consistent.

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

Prepare environment/appsettings values (especially PostgreSQL + Keycloak) before startup.

### 2) Run API + Gateway locally

- API defaults
  - HTTP: `http://localhost:5050`
  - HTTPS: `https://localhost:7050`
- Gateway defaults
  - HTTP: `http://localhost:8080`
  - HTTPS: `https://localhost:8443`

Swagger is available in development mode (or when explicitly enabled by feature flag).

### 3) Frontend (`frontend/`)

Use the following concrete flow from the repository root. The frontend smoke scripts expect the gateway to be reachable at `http://localhost:8080` unless you override the `--base-url` argument.

- Windows (PowerShell/CMD):
  - `cd frontend`
  - `npm install`
  - `npm run start` (development server; app calls gateway at `http://localhost:8080`)
  - `npm run build` (production build output)
  - `npm run smoke:gateway` (smoke check against gateway `http://localhost:8080`)
  - `npm run smoke:live-flows` (live flow smoke check against gateway `http://localhost:8080`)

- Bash users (Git Bash/WSL/Linux/macOS):
  - `cd frontend`
  - `npm install`
  - `npm run start` (development server; app calls gateway at `http://localhost:8080`)
  - `npm run build` (production build output)
  - `npm run smoke:gateway` (smoke check against gateway `http://localhost:8080`)
  - `npm run smoke:live-flows` (live flow smoke check against gateway `http://localhost:8080`)

Notes:
- If your gateway runs on another address, pass it explicitly, for example:
  - `npm run smoke:gateway -- --base-url http://localhost:8080`
  - `npm run smoke:live-flows -- --base-url http://localhost:8080`
- Keep API + Gateway running before frontend smoke checks.

### 4) Runtime probes

- Gateway/API liveness: `GET /health/live`
- Gateway/API readiness: `GET /health/ready`
- API operational detail (authenticated): `GET /api/health`

---

## Containerized Runtime

Use root `docker-compose.yml`.

### Standard stack

```bash
cp deploy/env/.env.local.example .env
docker compose up --build
```

> Default compose stack expects **external PostgreSQL + external Keycloak**. The base file does not provision these services; you must point the API/Gateway to reachable instances.
> In container networks, downstream host values must use the service name (for example `api`) rather than `localhost`.

### Required environment variables

At minimum, define:

- `API_CONNECTION_STRING`
- `JWT_AUTHORITY`
- `JWT_AUDIENCE`

Set these when your runtime scenario requires them:

- `REDIS_CONFIGURATION`, `REDIS_INSTANCE_NAME` (if Redis profile/feature is enabled)
- `API_SWAGGER_ENABLED`
- `API_APPLY_MIGRATIONS_ON_STARTUP`

### With Redis profile enabled

```bash
docker compose --profile redis up --build
```

### Opsiyonel olarak PostgreSQL/Keycloak’ı compose’a eklemek istersen aşağıdaki service bloklarını ekleyebilirsin

If you want Compose-managed PostgreSQL/Keycloak for local development, keep them in a local override file (for example `docker-compose.override.yml`) instead of the shared base file.

```yaml
services:
  postgres:
    image: postgres:16-alpine
    restart: unless-stopped
    environment:
      POSTGRES_DB: ${POSTGRES_DB:-qphising}
      POSTGRES_USER: ${POSTGRES_USER:-qphising}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-qphising-dev-password}
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      - qphising-postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-qphising} -d ${POSTGRES_DB:-qphising}"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    environment:
      API_CONNECTION_STRING: Host=postgres;Port=5432;Database=${POSTGRES_DB:-qphising};Username=${POSTGRES_USER:-qphising};Password=${POSTGRES_PASSWORD:-qphising-dev-password}
      Authentication__Jwt__Authority: http://keycloak:8080/realms/${KEYCLOAK_REALM:-QPhising}
    depends_on:
      postgres:
        condition: service_healthy

  keycloak:
    image: quay.io/keycloak/keycloak:25.0
    restart: unless-stopped
    command: ["start-dev", "--http-port=8080"]
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/${KEYCLOAK_DB:-keycloak}
      KC_DB_USERNAME: ${POSTGRES_USER:-qphising}
      KC_DB_PASSWORD: ${POSTGRES_PASSWORD:-qphising-dev-password}
      KC_BOOTSTRAP_ADMIN_USERNAME: ${KEYCLOAK_ADMIN:-admin}
      KC_BOOTSTRAP_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD:-admin}
    ports:
      - "${KEYCLOAK_PORT:-6060}:8080"
    depends_on:
      postgres:
        condition: service_healthy

  gateway:
    environment:
      Authentication__Jwt__Authority: http://keycloak:8080/realms/${KEYCLOAK_REALM:-QPhising}

volumes:
  qphising-postgres-data:
```

### Health endpoint verification (Gateway + API)

After the stack is up, verify both liveness and readiness endpoints:

```bash
curl -f http://localhost:5050/health/live
curl -f http://localhost:5050/health/ready
curl -f http://localhost:8080/health/live
curl -f http://localhost:8080/health/ready
```

### Exposed services

- Frontend: `http://localhost:4200`
- API: `http://localhost:5050`
- Gateway: `http://localhost:8080`
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

- Setup wizard/runtime configuration endpoints were removed on 2026-04-23; app startup now relies on static configuration sources.
- Redis is optional unless explicitly required by a feature/profile.
- Main management and analytics endpoints are protected; public tracking routes are intentionally constrained and rate-limited.
- Correlation IDs are propagated API ↔ Gateway for traceability.

---

## Documentation Index

For deeper implementation details, start with:

- `docs/architecture/` — system design, architecture boundaries, and contract-oriented references
- `docs/operations/` — runtime operations, runbooks, and incident handling guidance
- `docs/adr/` — Architecture Decision Records (ADRs) and decision history

---

## Licensing

QPhising is source-available under the QPhising Community License.

Commercial use requires a separate license.
See:
- LICENSE.md
- TRADEMARK.md
- COMMERCIAL.md
