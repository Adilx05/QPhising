# QPhising

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-19-DD0031?logo=angular&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![License](https://img.shields.io/badge/License-QPhising%20Community%201.0-blue)

QPhising is a backend-first **phishing awareness simulation platform** for running controlled security awareness campaigns, tracking visitor interactions on simulated landing pages, and generating analytics reports.

Built with Clean Architecture, CQRS, and a role-based multi-tenant design.

---

## Features

**Campaign Lifecycle Management**
- Full state machine: `Draft → Scheduled → Active → Paused → Completed / Cancelled`
- Edit campaign name, schedule window, and landing page HTML inline

**Tracking Pages**
- CRUD with publish/archive lifecycle
- Public slug-based and ID-based resolution (`/p/{slug}`)

**Visit Ingestion & Analytics**
- Anonymous visit ingestion with configurable deduplication
- Privacy controls: IP capture toggle, IP hash policy, bot filtering
- Analytics: total/unique visits, trend buckets, top pages, recent visit stream, referrer/user-agent analysis
- CSV and PDF report export (summary or detailed, per page or global)

**Identity & Access Control**
- OIDC Authorization Code + PKCE flow (no external OIDC library)
- JWT Bearer validation with Keycloak
- Role hierarchy: `Admin ≥ Operator ≥ Viewer`
- Silent token refresh with refresh_token grant

**Security**
- Rate limiting on public tracking endpoints
- Security audit logging (401/403/429 and domain events)
- Soft-delete on core entities
- ProblemDetails standardized error responses

**Operational**
- API + Gateway health model (liveness/readiness probes)
- Correlation ID propagation across requests
- Structured JSON logging
- Auto-migration on startup

---

## Architecture

```
┌──────────┐     ┌──────────┐     ┌──────────────────┐
│ Browser  │────▶│  Ocelot  │────▶│  API (.NET 10)   │
│ (Angular)│     │  Gateway  │     │  ASP.NET Core    │
└──────────┘     └──────────┘     └────────┬─────────┘
       │                                    │
       │ OIDC + PKCE                        │ CQRS + MediatR
       ▼                                    ▼
   ┌──────────┐                    ┌────────────────┐
   │ Keycloak │                    │  PostgreSQL    │
   │   IDP    │                    │  (EF Core)     │
   └──────────┘                    └────────────────┘
```

### Layered Structure

| Layer | Project | Responsibility |
|-------|---------|---------------|
| **API** | `QPhising.Api` | HTTP transport, middleware, EF Core persistence, health checks, rate limiting |
| **Application** | `QPhising.Application` | CQRS handlers, validators, contracts, authorization pipeline |
| **Domain** | `QPhising.Domain` | Pure business logic: aggregates, value objects, enums, policies |
| **Gateway** | `QPhising.Gateway` | Ocelot routing, auth forwarding, claims-to-headers middleware |
| **Tests** | `QPhising.Api.Tests` | Unit + integration tests |

### Frontend

- Angular 19 standalone components with feature-based modules
- PrimeNG UI components + TailwindCSS styling
- Generated OpenAPI proxy clients (`src/app/shared/proxy/`)
- Custom OIDC auth service (Authorization Code + PKCE) with silent token refresh
- Runtime-configurable API/auth endpoints via `runtime-config.js`

---

## Tech Stack

| Area | Technologies |
|---|---|
| **Backend** | .NET 10, ASP.NET Core, MediatR 12, FluentValidation, AutoMapper |
| **Data** | EF Core 10, Npgsql, PostgreSQL 16 |
| **Gateway** | Ocelot |
| **Frontend** | Angular 19.2, TypeScript, PrimeNG 19, TailwindCSS 3.4, Axios |
| **Auth** | OIDC (Authorization Code + PKCE), JWT Bearer, Keycloak |
| **Reporting** | QuestPDF, CSV generation |
| **Infrastructure** | Docker, Docker Compose, Nginx, Redis (optional) |
| **CI/CD** | GitHub Actions (build, test, quality gates, release) |
| **Tooling** | OpenAPI proxy generation (`openapi-typescript-codegen`), Swagger quality checks |

---

## Repository Structure

```
.
├── backend/
│   ├── API/                     # HTTP API, middleware, EF Core, migrations, health
│   ├── API.Tests/               # Unit + integration tests
│   ├── Application/             # CQRS commands/queries, handlers, validators
│   ├── Domain/                  # Aggregates, value objects, enums, policies
│   ├── Gateway/                 # Ocelot gateway, edge middleware
│   └── Infrastructure/          # (reserved — currently empty)
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/            # Auth, guards, config, HTTP, UI state
│   │   │   ├── features/        # Campaigns, tracking, templates, reports, audit, dashboard
│   │   │   └── shared/proxy/    # Generated OpenAPI client SDK
│   │   ├── environments/
│   │   └── assets/
│   └── docker/                  # Nginx config, runtime-config entrypoint
├── docs/                        # Architecture docs, ADRs, runbooks, GitHub Pages
├── scripts/                     # Quality gates, proxy generation, smoke tests
├── deploy/env/                  # Environment templates (.env.local, .staging, .production)
├── .github/workflows/           # CI, release, GitHub Pages
├── docker-compose.yml
├── QPhising.slnx
├── AGENTS.md                    # AI agent operating guidelines
├── TASKS.md                     # Implementation task tracking
├── LICENSE.md
├── COMMERCIAL.md
└── TRADEMARK.md
```

---

## Getting Started

### Prerequisites

- .NET SDK 10.x
- Node.js 20+ (22 used in Docker build)
- PostgreSQL 16+
- Keycloak instance with a configured realm and client
- (Optional) Redis for caching and rate limiting

### Local Development

#### 1. Configure Backend

Edit `backend/API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=qphising;Username=postgres;Password=yourpassword"
  },
  "Authentication": {
    "Jwt": {
      "Authority": "http://localhost:6060/realms/QPhising",
      "RequireHttpsMetadata": false
    }
  }
}
```

#### 2. Run Backend + Gateway

```bash
dotnet restore QPhising.slnx
dotnet build QPhising.slnx

# Terminal 1: API
dotnet run --project backend/API/QPhising.Api.csproj

# Terminal 2: Gateway
dotnet run --project backend/Gateway/QPhising.Gateway.csproj
```

| Service | URL |
|---------|-----|
| API | `http://localhost:5050` |
| Gateway | `http://localhost:8080` |
| Swagger | `http://localhost:5050/swagger` |

#### 3. Run Frontend

```bash
cd frontend
npm ci
npm run start
```

Frontend dev server: `http://localhost:4200`

### Docker

```bash
cp deploy/env/.env.local.example .env
docker compose up --build

# With Redis:
docker compose --profile redis up --build
```

Compose services: `api`, `gateway`, `frontend`, `redis` (profile-based).

> PostgreSQL and Keycloak are expected externally — add local overrides as needed.

---

## Configuration

### Environment Variables

| Variable | Service | Purpose |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | API | PostgreSQL connection string |
| `Authentication__Jwt__Authority` | API, Gateway | Keycloak OIDC authority URL |
| `Authentication__Jwt__Audience` | API, Gateway | Expected JWT audience |
| `Authentication__Jwt__RequireHttpsMetadata` | API, Gateway | Disable for local HTTP dev |
| `FeatureFlags__SwaggerEnabled` | API | Enable Swagger outside dev |
| `Database__ApplyMigrationsOnStartup` | API | Auto-run EF Core migrations |
| `HealthChecks__Redis__Enabled` | API | Toggle Redis health probe |
| `HealthChecks__Keycloak__Enabled` | API | Toggle Keycloak health probe |
| `Tracking__VisitorPrivacy__HashPepper` | API | Pepper for IP hashing |
| `Redis__Configuration` | API | Redis endpoint |
| `Gateway__RoutePolicies__ForwardAccessToken` | Gateway | Forward Bearer token to API |
| `Gateway__RoutePolicies__DownstreamApiHost` | Gateway | Downstream API hostname |
| `Gateway__RoutePolicies__DownstreamApiPort` | Gateway | Downstream API port |
| `QPHISING_API_BASE_URL` | Frontend | Gateway base URL |
| `QPHISING_AUTHORITY` | Frontend | Keycloak base URL |
| `QPHISING_REALM` | Frontend | Keycloak realm |
| `QPHISING_CLIENT_ID` | Frontend | OIDC client ID |
| `QPHISING_AUTH_SCOPE` | Frontend | Requested OIDC scopes |

### Frontend Runtime Config

At container startup, `docker/entrypoint.sh` generates a `runtime-config.js` from environment variables. Values are also overridable via `window.__QPHISING_*` globals.

---

## Database

- **Provider:** PostgreSQL via Npgsql
- **ORM:** EF Core with migrations
- **Auto-migration:** Configurable via `Database.ApplyMigrationsOnStartup`
- **Soft delete:** Core entities use `AuditableSoftDeletableEntity` with global query filters

### Migration Commands

```bash
dotnet ef migrations add <Name> --project backend/API --startup-project backend/API
dotnet ef database update --project backend/API --startup-project backend/API
```

---

## Authentication Flow

```
1. User visits protected route
2. Auth guard checks session → redirects to Keycloak
3. Keycloak issues auth code → callback at /auth/callback
4. Frontend exchanges code + PKCE verifier for tokens
5. Access token stored in sessionStorage, injected into every API call
6. Token refreshed silently via refresh_token grant before expiry
7. On 401 with expired refresh token → redirect to Keycloak login
```

**Authorization policies:** `AdminOnly`, `OperatorOrAbove`, `ViewerOrAbove` — enforced at both API and route guard levels.

---

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `GET` | `/api/campaigns` | Viewer+ | List campaigns |
| `GET` | `/api/campaigns/{id}` | Viewer+ | Get campaign |
| `POST` | `/api/campaigns` | Operator+ | Create campaign |
| `PUT` | `/api/campaigns/{id}` | Operator+ | Update campaign |
| `DELETE` | `/api/campaigns/{id}` | Admin | Delete campaign |
| `POST` | `/api/campaigns/{id}/schedule` | Operator+ | Set campaign schedule |
| `POST` | `/api/campaigns/{id}/start` | Operator+ | Start campaign |
| `POST` | `/api/campaigns/{id}/pause` | Operator+ | Pause campaign |
| `POST` | `/api/campaigns/{id}/complete` | Operator+ | Complete campaign |
| `POST` | `/api/campaigns/{id}/cancel` | Admin | Cancel campaign |
| `GET` | `/api/tracking/pages` | Viewer+ | List tracking pages |
| `GET` | `/api/tracking/pages/{id}` | Viewer+ | Get tracking page |
| `PUT` | `/api/tracking/pages/{id}` | Operator+ | Update tracking page |
| `POST` | `/api/tracking/pages` | Operator+ | Create tracking page |
| `POST` | `/api/tracking/pages/{id}/publish` | Operator+ | Publish |
| `POST` | `/api/tracking/pages/{id}/archive` | Operator+ | Archive |
| `DELETE` | `/api/tracking/pages/{id}` | Admin | Delete tracking page |
| `GET` | `/api/tracking/pages/{id}/analytics` | Viewer+ | Page analytics |
| `GET` | `/api/tracking/analytics/overview` | Viewer+ | Global analytics overview |
| `GET` | `/api/templates` | Viewer+ | List templates |
| `POST` | `/api/templates` | Operator+ | Create template |
| `PUT` | `/api/templates/{id}` | Operator+ | Update template |
| `DELETE` | `/api/templates/{id}` | Operator+ | Delete template |
| `GET` | `/api/audit/logs` | Operator+ | Query audit logs |
| `GET` | `/api/reports/export` | Viewer+ | Export CSV/PDF report |
| `GET` | `/p/{slug}` | Public | Public landing page |
| `POST` | `/api/tracking/pages/{slug}/visits` | Public | Ingest visit |
| `GET` | `/health/live` | Public | Liveness probe |
| `GET` | `/health/ready` | Public | Readiness probe |

Full OpenAPI spec available at `/swagger/v1/swagger.json` when Swagger is enabled.

---

## Workflow

Typical operator/admin flow:

1. Sign in via Keycloak
2. Create HTML templates (optional)
3. Create tracking pages (slug, title, HTML content, privacy settings)
4. Create campaigns linked to tracking pages
5. Schedule, start, and manage campaign lifecycle
6. Share the public `/p/{slug}` URL for the simulation
7. Monitor dashboard KPIs and per-page analytics
8. Export CSV/PDF reports
9. Review audit logs for security events

---

## Quality Gates

The CI pipeline enforces:

- Backend build + tests with coverage
- Frontend build
- Swagger endpoint accessibility
- OpenAPI spec matches generated proxies (determinism check)
- Proxy client matches gateway routes
- Frontend → gateway smoke tests

Run locally:

```bash
# Backend
dotnet test backend/API.Tests/QPhising.Api.Tests.csproj

# Quality scripts
node scripts/check-swagger-quality.js
node scripts/validate-proxy-generation.js
node scripts/check-gateway-swagger-alignment.js
node scripts/check-frontend-gateway-smoke.js

# Frontend
cd frontend && npm run build
```

---

## Security

- JWT + Keycloak for all protected endpoints
- Rate limiting on public tracking routes (120/min landing, 60/min visit ingestion)
- IP hashing with configurable pepper for visitor privacy
- Soft-delete prevents data loss
- Correlation ID tracing across requests
- Structured audit logging of auth failures and domain operations
- No real secrets committed (runtime config excluded via `.gitignore`)

---

## License

QPhising Community License 1.0 — see [LICENSE.md](LICENSE.md).

- Non-commercial use, modification, self-hosting, and redistribution permitted
- Commercial use requires a separate commercial license (see [COMMERCIAL.md](COMMERCIAL.md))
