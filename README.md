# QPhising

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-19-DD0031?logo=angular&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Gateway](https://img.shields.io/badge/Ocelot-Gateway-0A66C2)
![License](https://img.shields.io/badge/License-QPhising%20Community%201.0-blue)

QPhising is a backend-first **web tracking, visitor analytics, and campaign simulation operations platform** built with Clean Architecture.

It provides:

- Public tracking pages (`/p/{slug}`) that can be resolved and monitored in real time,
- Anonymous visit ingestion with deduplication and privacy controls,
- Analytics dashboards (total visits, unique visitors, trends, top pages, recent visits),
- CSV/PDF report export capabilities,
- Role-based administrative management for campaigns, templates, tracking pages, and audit logs.

---

## Overview

QPhising is designed for awareness/simulation/monitoring use-cases where organizations need to:

- Create and lifecycle-manage campaigns,
- Bind campaigns to real tracking pages,
- Track visitor behavior through public landing surfaces,
- Observe analytics trends and operational health,
- Export analytics evidence for reporting.

The platform enforces backend contracts first (Swagger/OpenAPI), then consumes those contracts in Angular through generated proxies.

> **Important:** This README reflects implemented behavior from the current repository state. Items marked as **(inferred)** are derived from code/config conventions where explicit docs are minimal.

---

## Features

- Campaign lifecycle management (`Draft`, `Scheduled`, `Active`, `Paused`, `Completed`, `Cancelled`)
- Tracking page CRUD + publish/archive lifecycle
- Public landing resolution via slug (`GET /p/{slug}`)
- Anonymous visit ingestion by tracking-page ID and slug
- Visit deduplication guard within configurable window
- Visitor privacy controls (IP capture on/off and hash policy)
- Bot traffic filtering toggle in analytics/report pipelines
- Tracking analytics overview (global KPIs, top pages, trend buckets, recent stream)
- Tracking analytics detail per page (summary, trend, filtered recent events)
- Report center with CSV/PDF export (global or selected tracking page, summary/detailed)
- Template management for HTML landing content
- Audit log query UI/API for security and operational events
- JWT authentication (Keycloak authority) + role-based authorization (`Admin`, `Operator`, `Viewer`)
- API/Gateway health model with liveness/readiness endpoints
- Structured JSON logging + correlation ID propagation
- Soft-delete semantics for core entities
- Contract quality gates (Swagger checks, proxy determinism, gateway/proxy consistency scripts)

---

## Architecture

QPhising follows Clean Architecture boundaries:

### Frontend (`frontend/`)

- Angular 19 standalone app
- Feature-based modules (dashboard, campaigns, tracking, templates, reports, audit)
- Generated TypeScript clients under `src/app/shared/proxy`
- Runtime-configurable API/auth endpoints injected at container startup (`runtime-config.js`)

### API (`backend/API/`)

- ASP.NET Core Web API
- Controllers contain transport concerns only
- CQRS orchestration via MediatR handlers in Application layer
- ProblemDetails middleware for standardized API errors
- Rate limiting on public tracking endpoints

### Application (`backend/Application/`)

- Use-cases: commands/queries + validators
- Authorization behavior pipeline
- DTO contracts for frontend/API boundaries
- Reporting query orchestration and exporter abstraction

### Domain (`backend/Domain/`)

- Pure business rules for Campaign, Tracking, Template, Identity models
- Aggregates, value objects, enums, policies
- No infrastructure/framework dependencies

### Infrastructure/Persistence (inside API project)

- EF Core DbContext and repository implementations
- PostgreSQL persistence mappings and migrations
- Audit log persistence and health checks

### Gateway (`backend/Gateway/`)

- Ocelot API gateway
- Route forwarding to downstream API
- Auth forwarding / claim-to-header forwarding middleware
- Independent liveness/readiness probes

### Database

- PostgreSQL with EF Core migration-based schema management
- Core tables: `campaigns`, `tracking_pages`, `visit_events`, `templates`, `audit_log_entries`

---

## Tech Stack

| Area | Technologies |
|---|---|
| Backend | .NET 10, ASP.NET Core, MediatR, FluentValidation, AutoMapper |
| Data | EF Core, Npgsql, PostgreSQL |
| Gateway | Ocelot |
| Frontend | Angular 19, TypeScript, PrimeNG, TailwindCSS |
| Auth | JWT Bearer (Keycloak authority) |
| Reporting | QuestPDF, CSV generation |
| Quality/Tooling | Node.js scripts, OpenAPI proxy generation (`openapi-typescript-codegen`) |
| CI/CD | GitHub Actions (`ci.yml`, `release.yml`) |
| Containers | Docker, Docker Compose, Nginx (frontend runtime) |

---

## Repository Structure

```text
.
├─ backend/
│  ├─ API/                # HTTP API, middleware, EF Core persistence, health checks
│  ├─ Application/        # CQRS handlers, validators, contracts, mapping
│  ├─ Domain/             # Aggregates, value objects, domain enums/policies
│  ├─ Gateway/            # Ocelot gateway and edge middleware
│  └─ API.Tests/          # Unit + integration tests
├─ frontend/
│  ├─ src/app/core/       # auth, guards, config, shared UI state
│  ├─ src/app/features/   # dashboard, tracking, campaigns, templates, reports, audit
│  ├─ src/app/shared/proxy/ # generated OpenAPI clients
│  └─ docker/             # nginx + runtime-config entrypoint
├─ docs/
│  ├─ architecture/
│  └─ operations/
├─ deploy/env/            # local/staging/production env templates
├─ scripts/               # quality gates, smoke checks, proxy generation
├─ docker-compose.yml
├─ QPhising.slnx
└─ LICENSE.md
```

---

## Getting Started

### Requirements

- .NET SDK 10.x
- Node.js 20+ (Node 22 used in frontend Docker build)
- npm
- PostgreSQL 16+ (or compatible)
- Keycloak realm/client for JWT issuance/validation
- (Optional) Redis for optional readiness/degradation scenarios
- Docker + Docker Compose (for containerized runs)

### Local Development

#### 1) Configure backend settings

Update `backend/API/appsettings.Development.json` and `backend/Gateway/appsettings.Development.json` (or environment overrides) for:

- PostgreSQL connection string,
- JWT authority/audience,
- Optional Redis,
- CORS origins.

#### 2) Run backend + gateway

```bash
dotnet restore QPhising.slnx
dotnet build QPhising.slnx

# Terminal 1
dotnet run --project backend/API/QPhising.Api.csproj

# Terminal 2
dotnet run --project backend/Gateway/QPhising.Gateway.csproj
```

Default local endpoints:

- API HTTP: `http://localhost:5050`
- Gateway HTTP: `http://localhost:8080`

#### 3) Run frontend

```bash
cd frontend
npm ci
npm run start
```

Frontend dev server defaults to Angular local runtime and targets gateway base URL from environment/runtime config.

### Docker Setup

#### Base stack

```bash
cp deploy/env/.env.local.example .env
docker compose up --build
```

Services in base compose:

- `api`
- `gateway`
- `frontend`
- `redis` (profile-based, optional)

Run with Redis profile:

```bash
docker compose --profile redis up --build
```

> The base compose expects external PostgreSQL/Keycloak unless you add local overrides.

### Environment Variables

Detected from `docker-compose.yml`, env templates, and frontend runtime entrypoint.

| Variable | Used By | Purpose |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | API, Gateway | Runtime environment (`Development`, `Staging`, `Production`) |
| `API_CONNECTION_STRING` | API | EF Core/PostgreSQL connection string (`ConnectionStrings__DefaultConnection`) |
| `JWT_AUTHORITY` | API, Gateway | OIDC/JWT authority URL |
| `JWT_AUDIENCE` | API, Gateway | Expected JWT audience |
| `JWT_REQUIRE_HTTPS_METADATA` | API, Gateway | Enables/disables metadata HTTPS requirement |
| `API_HEALTHCHECK_KEYCLOAK_ENABLED` | API | Toggle Keycloak readiness probe |
| `API_SWAGGER_ENABLED` | API | Enables Swagger outside development |
| `API_APPLY_MIGRATIONS_ON_STARTUP` | API | Startup migration check/apply toggle |
| `TRACKING_HASH_PEPPER` | API | Optional pepper for visitor IP hashing |
| `REDIS_CONFIGURATION` | API | Redis endpoint |
| `REDIS_INSTANCE_NAME` | API | Redis instance prefix |
| `REDIS_PORT` | Compose | Redis host port mapping |
| `API_PORT` | Compose | API exposed port |
| `GATEWAY_PORT` | Compose | Gateway exposed port |
| `FRONTEND_PORT` | Compose | Frontend exposed port |
| `GATEWAY_FORWARD_ACCESS_TOKEN` | Gateway | Forward bearer token to downstream (policy setting) |
| `GATEWAY_DOWNSTREAM_API_HOST` | Gateway | Downstream API host for readiness probe |
| `GATEWAY_DOWNSTREAM_API_PORT` | Gateway | Downstream API port for readiness probe |
| `GATEWAY_DOWNSTREAM_API_SCHEME` | Gateway | Downstream API scheme (`http`/`https`) |
| `FRONTEND_API_BASE_URL` / `QPHISING_API_BASE_URL` | Frontend | API base URL injected into `runtime-config.js` |
| `FRONTEND_AUTHORITY` / `QPHISING_AUTHORITY` | Frontend | OIDC authority base |
| `FRONTEND_REALM` / `QPHISING_REALM` | Frontend | Keycloak realm |
| `FRONTEND_CLIENT_ID` / `QPHISING_CLIENT_ID` | Frontend | OIDC client ID |
| `FRONTEND_AUTH_SCOPE` / `QPHISING_AUTH_SCOPE` | Frontend | Requested OIDC scopes |
| `FRONTEND_AUTH_REDIRECT_URI` / `QPHISING_AUTH_REDIRECT_URI` | Frontend | Login redirect URI |
| `FRONTEND_POST_LOGOUT_REDIRECT_URI` / `QPHISING_POST_LOGOUT_REDIRECT_URI` | Frontend | Post logout redirect URI |
| `SETUP_ALLOW_RUNTIME_OVERRIDES` | API, Gateway | Legacy guard flag still present in compose/env templates (inferred: compatibility) |

---

## Database

- Provider: PostgreSQL (Npgsql)
- ORM: EF Core
- Migration strategy: API performs migration check/apply during startup (`Database.Migrate()` in startup flow)
- Current migration file in repo: `20260423120123_firstinity`
- Soft delete is enforced on key write-side entities via `is_deleted` + global query filters

### Typical migration commands

```bash
# from repo root
dotnet ef migrations add <MigrationName> --project backend/API --startup-project backend/API
dotnet ef database update --project backend/API --startup-project backend/API
```

> Seeding: no broad demo-data seed pipeline is defined in current startup path (inferred from startup/db configuration).

---

## Authentication

- API and Gateway use JWT Bearer authentication.
- Authority/audience are read from configuration.
- Role policies are mapped to:
  - `AdminOnly`
  - `OperatorOrAbove`
  - `ViewerOrAbove`
- Frontend uses OIDC Authorization Code + PKCE flow:
  - Redirect to provider login,
  - Handle callback at `/auth/callback`,
  - Persist session in browser storage,
  - Apply route guards by role.

---

## Usage

Typical operator/admin workflow:

1. Sign in through configured OIDC provider.
2. Create/manage templates (optional HTML foundation).
3. Create tracking pages (slug/title/privacy/options).
4. Create campaigns linked to tracking pages.
5. Start/operate campaign lifecycle as needed.
6. Share public route (`/p/{slug}`) for monitored flow entry.
7. Observe dashboard metrics and tracking analytics details.
8. Export CSV/PDF reports from Report Center.
9. Review audit logs for security/operations traceability.

---

## Reports & Analytics

Implemented analytics surfaces include:

- **Overview KPIs:** total visits, unique visitors
- **Top pages:** ranking with total + unique counts
- **Recent visit stream:** newest events with referrer/user agent context
- **Trend buckets:** time-windowed aggregation (overview + page analytics)
- **Filters:** date range, bot exclusion, trend bucket granularity, local UI filters for referrer/user-agent buckets
- **Exports:**
  - Scope: global or selected tracking page
  - Detail level: summary/detailed
  - Format: CSV/PDF
  - Locale option: TR/EN

---

## Build & Production

### Build locally

```bash
# Backend
dotnet restore QPhising.slnx
dotnet build QPhising.slnx --configuration Release
dotnet test backend/API.Tests/QPhising.Api.Tests.csproj --configuration Release

# Frontend
cd frontend
npm ci
npm run build
```

### CI flow (`.github/workflows/ci.yml`)

- Restore/build backend
- Run backend tests with coverage
- Build frontend
- Run frontend UI smoke checks
- Run Swagger quality gates
- Validate proxy generation determinism
- Verify gateway and proxy consistency

### Release baseline (`.github/workflows/release.yml`)

Manual dispatch with:

- `target_environment` (`staging` or `production`)
- `release_version` (`vMAJOR.MINOR.PATCH`)

Workflow builds/tests and creates an artifact bundle (`backend`, `frontend`, `docker-compose.yml`, `deploy/env`).

---

## Security Notes

- Enforce HTTPS and strict Keycloak metadata validation in non-local environments.
- Keep JWT authority/audience consistent across API, gateway, and frontend runtime config.
- Restrict CORS origins to known frontend hosts.
- Use strong secrets for database/auth and do not commit real secrets.
- Configure `TRACKING_HASH_PEPPER` in staging/production for stronger hash resilience.
- Maintain rate limits on public tracking routes to reduce abuse risk.
- Review audit logs (`/api/audit/logs`) regularly for 401/403/429 and sensitive operations.

---

## Roadmap

Potential next steps aligned with current architecture:

- Add richer analytics visualizations (native chart components) across dashboards.
- Expand export scheduler/automation (periodic report jobs).
- Add retention enforcement/background cleanup jobs for visit data.
- Introduce dedicated infrastructure project separation for persistence/integrations.
- Add explicit deployment manifests (Kubernetes/Helm) when required.
- Extend observability with tracing/metrics backends (OpenTelemetry) (inferred).

---

## Contributing

1. Fork the repository and create a feature branch.
2. Follow Clean Architecture boundaries and CQRS patterns already used.
3. Keep API contracts authoritative; regenerate/check proxies when contracts change.
4. Run local checks before PR:
   - backend build/tests
   - frontend build
   - swagger/proxy/gateway validation scripts
5. Submit a PR with clear implementation notes and impact scope.

---

## License

This project is licensed under **QPhising Community License 1.0**.

- Non-commercial use, modification, self-hosting, and redistribution are allowed.
- Commercial use (selling, paid SaaS, paid embedding, monetized offerings) requires a separate commercial license.

See:

- `LICENSE.md`
- `COMMERCIAL.md`

