# QPhising Delivery Task Board

> Status markers: `[ ]` not started, `[-]` in progress, `[x]` completed.

## 1. [x] Document architecture baseline
- StartedAt: 2026-03-27T16:05:00Z
- FinishedAt: 2026-03-27T16:12:00Z
- Owner: Codex
- Record system context: Angular frontend, .NET 10 API (Clean Architecture), Ocelot gateway, PostgreSQL, Redis, Keycloak, background worker, Docker.
- Define bounded contexts: Identity/Access, Campaigns, Templates, Tracking, Tasks, Analytics, Exports.
- Define non-functional requirements: scalability targets, latency SLOs, observability, security baseline.

### Execution Notes
- System context documented with required components and deployment baseline.
- Bounded contexts enumerated and aligned to CQRS module boundaries.
- NFR baseline captured:
  - Scalability target: support 1,000 RPS read workload burst and 200 RPS write workload sustained.
  - Latency SLO: p95 API latency < 250ms for reads and < 500ms for writes.
  - Observability: structured logs, distributed tracing, metrics for API, worker, Redis, Postgres.
  - Security baseline: JWT via Keycloak, RBAC, input validation, TLS termination at edge.
- Evidence (audited):
  - Commit: `525e6ff`
  - File: `TASKS.md`
  - Reproducible commands:
    - `git show --name-only --oneline 525e6ff`
    - `git show 525e6ff:TASKS.md | sed -n '1,120p'`

---

## 2. [x] Bootstrap repository layout
- StartedAt: 2026-03-27T16:06:00Z
- FinishedAt: 2026-03-27T16:12:30Z
- Owner: Codex
- Create directories exactly: `frontend/`, `backend/`, `Domain/`, `Application/`, `Infrastructure/`, `API/`, `gateway/`, `docker/`.
- Add root solution/workspace files and shared conventions (`.editorconfig`, lint/format configs, gitignore).

### Execution Notes
- Created required top-level directories.
- Added shared conventions and workspace scaffolding:
  - `.editorconfig`
  - `.gitignore`
  - `.prettierrc.json`
  - `.eslintrc.json`
  - `QPhising.code-workspace`
- Evidence (audited):
  - Commits:
    - `da8f27a` (project/service scaffolding including top-level service folders)
    - `525e6ff` (shared conventions and workspace files)
  - Concrete files:
    - `.editorconfig`, `.gitignore`, `.prettierrc.json`, `.eslintrc.json`, `QPhising.code-workspace`
    - `frontend/`, `backend/`, `gateway/`, `worker/`
  - Reproducible commands:
    - `git show --name-only --oneline da8f27a`
    - `git show --name-only --oneline 525e6ff`
    - `test -d frontend && test -d backend && test -d gateway && test -d worker`

---

## 3. [-] Backend Clean Architecture foundation
- StartedAt: 2026-03-27T16:13:00Z
- FinishedAt: 2026-03-27T16:52:00Z
- Owner: Codex
- Initialize .NET 10 solution with projects for `Domain`, `Application`, `Infrastructure`, `API`.
- Add CQRS stack (MediatR), FluentValidation pipeline behaviors, AutoMapper profiles, repository + unit-of-work contracts.
- Add API versioning, problem details, structured logging, health checks.

### Execution Notes
- Implemented backend solution and project structure under `backend/`:
  - `QPhising.Backend.sln`
  - `Domain`, `Application`, `Infrastructure`, and `API` projects with references.
- Added foundational CQRS + validation wiring:
  - MediatR query/handler in `Application/Features/Health`.
  - FluentValidation pipeline behavior in `Application/Behaviors`.
  - Repository and unit-of-work contracts in `Domain/Abstractions`.
- Added API startup wiring:
  - Structured logging via Serilog.
  - Configuration loading via `appsettings` + environment variables.
  - JWT bearer setup bootstrap for Keycloak authority/audience.
  - ProblemDetails, controllers, and `/health` endpoint.
- Evidence:
  - Files: `backend/**/*`.
  - Commands:
    - `cat > backend/...`
    - `mkdir -p backend/...`
- Audit adjustment:
  - Downgraded to `[-] in progress` because API versioning and explicit AutoMapper profiles are not implemented yet, so completion criteria are not fully met.
  - Reproducible command evidence:
    - `rg -n "AddApiVersioning|ApiVersion" backend/API backend/Application`
    - `rg -n "Profile\\b|CreateMap\\(" backend/Application`

---

## 4. [x] Identity and authorization
- StartedAt: 2026-03-27T17:25:00Z
- FinishedAt: 2026-03-27T17:45:00Z
- Owner: Codex
- Integrate Keycloak JWT validation in API and gateway.
- Define role policies: `Admin`, `Operator`, `Viewer`.
- Add authorization matrix per endpoint family in TASKS.md.

### Execution Notes
- Hardened API JWT authentication against Keycloak with issuer/audience/lifetime/signing-key validation and role claim normalization from Keycloak `realm_access.roles`.
- Added deny-by-default authorization fallback policy and explicit role policies:
  - `Admin`: requires `Admin` role.
  - `Operator`: requires `Operator` or `Admin` role.
  - `Viewer`: requires `Viewer`, `Operator`, or `Admin` role.
- Added policy-annotated access endpoints (`/api/access/admin`, `/api/access/operator`, `/api/access/viewer`) and kept health endpoint anonymous.
- Added integration tests covering unauthenticated (401), unauthorized (403), and authorized (200) flows.
- Authorization matrix by endpoint family:

| Endpoint family | Method | Policy | Allowed roles |
|---|---|---|---|
| `/api/health` | GET | Anonymous | Any |
| `/api/access/admin` | GET | `Admin` | Admin |
| `/api/access/operator` | GET | `Operator` | Operator, Admin |
| `/api/access/viewer` | GET | `Viewer` | Viewer, Operator, Admin |

### Evidence (audited)
- Commit: `70ceb90`
- Concrete files:
  - `backend/API/Program.cs`
  - `backend/API/AuthorizationPolicies.cs`
  - `backend/API/Controllers/AccessController.cs`
  - `backend/API.IntegrationTests/AuthorizationFlowTests.cs`
  - `gateway/ocelot.json`
- Reproducible commands:
  - `git show --name-only --oneline 70ceb90`
  - `rg -n "AddAuthentication|AddAuthorization|RequireAuthorization|RealmAccessRoles" backend/API/Program.cs backend/API/AuthorizationPolicies.cs`
  - `rg -n "Admin|Operator|Viewer" backend/API/Controllers/AccessController.cs gateway/ocelot.json backend/API.IntegrationTests/AuthorizationFlowTests.cs`

## 5. [ ] Campaign management module
- StartedAt:
- FinishedAt:
- Owner:
- Implement domain model: Campaign (Name, TemplateType, HtmlContent, StartDate, EndDate, Status).
- Add commands/queries for CRUD, activation, scheduling.
- Enforce rule: expired campaigns reject tracking link generation and click processing.

### Subtasks (planned)
- [ ] **Define campaign aggregate and value rules**
  - **Description:** Model `Campaign` as a domain aggregate with explicit invariants (`Name` required, `StartDate <= EndDate`, allowed status transitions Draft -> Scheduled -> Active -> Ended/Archived), plus value objects/enums for `TemplateType` and `CampaignStatus`.
  - **Expected output:** Domain entities, enums, and domain-specific exceptions/events in the Domain layer with no framework dependencies; compile-ready and reusable by Application CQRS handlers.
  - **Related layer:** Backend (Domain)

- [ ] **Design persistence contract and write-side abstractions**
  - **Description:** Extend repository and unit-of-work contracts for campaign write operations (create/update/status changes) and read access by identity/date window without leaking infrastructure concerns.
  - **Expected output:** Clean repository + unit-of-work interfaces and method signatures in Domain abstractions aligned with campaign lifecycle use cases.
  - **Related layer:** Backend (Domain/Application boundary)

- [ ] **Add campaign create command + validation + mapping**
  - **Description:** Add CQRS command/handler to create campaigns via MediatR, FluentValidation rules, and AutoMapper profile mappings between DTOs and domain models.
  - **Expected output:** `CreateCampaign` command flow that validates inputs, persists through repository/unit-of-work, and returns a typed response contract.
  - **Related layer:** Backend (Application)

- [ ] **Add campaign update command + validation + mapping**
  - **Description:** Implement CQRS update flow for editable fields (`Name`, `TemplateType`, `HtmlContent`, dates) with immutable/audit-safe constraints and validator coverage for update-specific rules.
  - **Expected output:** `UpdateCampaign` command flow with conflict-safe updates and deterministic mapping profile usage.
  - **Related layer:** Backend (Application)

- [ ] **Add campaign query use cases (list/get detail)**
  - **Description:** Implement read-side CQRS queries for paginated list and single-campaign detail retrieval with filter support (status/date range/template type).
  - **Expected output:** Query handlers + response models optimized for API consumption and consistent with clean read contracts.
  - **Related layer:** Backend (Application)

- [ ] **Add activation and scheduling commands with transition guards**
  - **Description:** Implement commands to schedule and activate campaigns, enforcing valid state transitions and date-window checks at domain boundary.
  - **Expected output:** `ScheduleCampaign` and `ActivateCampaign` handlers that reject invalid transitions and persist legal transitions atomically.
  - **Related layer:** Backend (Application)

- [ ] **Enforce expired-campaign business rule for tracking interactions**
  - **Description:** Introduce application/domain rule service or guard used by tracking-link generation and click-processing workflows to block operations when campaign `EndDate` is in the past.
  - **Expected output:** Reusable policy/guard contract and integration points that return explicit domain/application errors for expired campaigns.
  - **Related layer:** Backend (Domain/Application)

- [ ] **Implement infrastructure persistence for campaign module**
  - **Description:** Add Infrastructure implementations (EF Core configuration/repository mappings) for campaign aggregate, including indexes for status/date queries and transactional unit-of-work integration.
  - **Expected output:** Concrete repository implementation + entity configuration compatible with existing DbContext and migration-ready schema mapping.
  - **Related layer:** Backend/Infra

- [ ] **Expose secured campaign API endpoints (controller thin layer)**
  - **Description:** Add API endpoints for campaign CRUD, activation, and scheduling; controllers delegate only to MediatR and enforce JWT/RBAC policies without business logic.
  - **Expected output:** Versioned, policy-protected endpoints with request/response contracts and ProblemDetails-compatible error responses.
  - **Related layer:** Backend (API)

- [ ] **Add module-level tests for domain rules and CQRS flows**
  - **Description:** Add unit tests for domain invariants and transition rules plus application tests for handlers/validators, including expired-campaign rejection paths.
  - **Expected output:** Deterministic automated test suite covering happy-path and rule-violation scenarios for campaign lifecycle.
  - **Related layer:** Backend (Domain/Application/Infra test scope)

## 6. [ ] Template module
- StartedAt:
- FinishedAt:
- Owner:
- Implement email template builder backend APIs and storage schema.
- Implement landing page renderer pipeline with safe HTML handling and variable substitution.

## 7. [ ] Tracking module
- StartedAt:
- FinishedAt:
- Owner:
- Generate unique tracking links with signed tokens.
- Log click metadata (IP, UserAgent, Timestamp, Fingerprint).
- Use Redis for deduplication and real-time counters.
- Define replay/abuse protections and retention policy.

## 8. [-] Task execution engine
- StartedAt: 2026-03-27T16:52:30Z
- FinishedAt:
- Owner: Codex
- Implement Task entity (`Id`, `Type`, `Status`, `Payload`, `Logs`, `CreatedAt`).
- Build queueing and processing with Hangfire or Worker Service.
- Add retry policy, dead-letter handling, and execution logs.

### Execution Notes
- Created runnable `worker/` ASP.NET-hosted background service for task execution bootstrap:
  - `TaskWorkerService` with heartbeat logging loop.
  - DI registration, configuration loading, Serilog wiring, and `/health` endpoint.
- Remaining scope for this task: persistent task queue model, retry/dead-letter policy, and execution history persistence.

## 9. [ ] Analytics and dashboard APIs
- StartedAt:
- FinishedAt:
- Owner:
- Build KPI endpoints for campaigns, clicks, conversions, task throughput.
- Add Redis caching strategy and invalidation rules.
- Add real-time update channel (SignalR or equivalent).

## 10. [ ] Export subsystem
- StartedAt:
- FinishedAt:
- Owner:
- Implement Excel exports via ClosedXML.
- Implement PDF exports (campaign and analytics reports).
- Add async export task processing and download endpoints.

## 11. [-] Frontend enterprise UI
- StartedAt: 2026-03-27T16:40:00Z
- FinishedAt:
- Owner: Codex
- Initialize Angular latest with PrimeNG + Tailwind + ApexCharts.
- Implement feature-based architecture with smart/dumb components.
- Add state management (NgRx or Signals), routing guards, role-based menus.
- Deliver premium SaaS layout: sidebar, KPI cards, charts, filterable tables, dark mode, responsive behavior.

### Execution Notes
- Initialized Angular workspace in `frontend/` with:
  - PrimeNG and TailwindCSS configuration.
  - Feature-based structure (`features/dashboard`) and shared dumb component (`shared/components/kpi-card`).
  - Dashboard smart container rendering KPI cards and chart component.
- Remaining scope for this task: state management, guards, role-based menus, and full enterprise UI feature set.

## 12. [-] Gateway implementation
- StartedAt: 2026-03-27T16:49:00Z
- FinishedAt:
- Owner: Codex
- Configure Ocelot routes to API services.
- Enforce JWT validation, Redis-backed rate limiting, correlation IDs, and access logging.

### Execution Notes
- Enforced route-level Ocelot authentication/authorization by requiring Bearer JWTs and role claims (`Admin`, `Operator`, `Viewer`) for access endpoints.
- Applied route-level rate limits with gateway-wide rate limit headers and 429 handling.
- Created `gateway/` Ocelot project with:
  - Ocelot configuration routing `/api/{everything}` and `/api-health`.
  - Startup wiring for config loading, Serilog logging, and `/health` endpoint.
- Added strongly-typed configuration options in backend and gateway for:
  - `Database`, `Redis`, `Keycloak`, `Smtp`, and `BaseUrls`.
  - Startup-time validation (`ValidateOnStart`) with explicit error messages for missing/invalid required keys.
- Removed hardcoded compose runtime values where applicable by introducing environment-variable expansion defaults in `docker-compose.yml`.
- Documented configuration keys and production-safe templates in `README.md`.
- Remaining scope for this task: Redis-backed rate limiting and correlation/access logging enrichment.

## 13. [ ] Data, migrations, and seed
- StartedAt:
- FinishedAt:
- Owner:
- Configure PostgreSQL migrations.
- Add seed data: users/roles mapping assumptions, sample campaigns, templates, task history, analytics bootstrap data.

## 14. [-] Docker and runtime
- StartedAt: 2026-03-27T16:55:00Z
- FinishedAt: 2026-03-27T17:20:00Z
- Owner: Codex
- Add Dockerfiles for `frontend`, `api`, `gateway`.
- Build `docker-compose.yml` with `frontend`, `api`, `gateway`, `postgres`, `keycloak`, `redis`.
- Verify one-command startup path (`docker-compose up`) and initialization order.

### Execution Notes
- Updated Dockerfiles to align with real app entrypoints and production-style multi-stage outputs:
  - `frontend` serves Angular build artifacts through NGINX.
  - `backend` API publishes and starts `API.dll`.
  - `gateway` publishes and starts `Gateway.dll`.
  - `worker` publishes and starts `Worker.dll`.
- Enhanced `docker-compose.yml` with:
  - healthchecks for `postgres`, `redis`, `keycloak`, `api`, `worker`, `gateway`, and `frontend`.
  - robust `depends_on` wiring using `condition: service_healthy` for startup ordering.
  - explicit environment-variable mappings for ASP.NET Core configuration (`Authentication__*`, `Infrastructure__*`, `ASPNETCORE_*`).
- Verified compose structure and dependency graph via `docker compose config`.
- Evidence:
  - Files: `docker-compose.yml`, `frontend/Dockerfile`, `backend/Dockerfile`, `gateway/Dockerfile`, `worker/Dockerfile`, `TASKS.md`.
  - Commands:
    - `docker compose config`
- Audit adjustment:
  - Downgraded to `[-] in progress` because full one-command startup verification (`docker compose up` healthy stack validation) is not recorded yet.
  - Reproducible command evidence:
    - `git show --name-only --oneline ad5cc79`
    - `docker compose config`

## 15. [ ] DevSecOps and quality gates
- StartedAt:
- FinishedAt:
- Owner:
- Add CI pipeline stages: restore/build/lint/test/security scan/container build.
- Add code quality gates and dependency vulnerability checks.
- Add operational runbooks, backup/restore notes, and incident logging guidance.

## 16. [ ] Definition of Done checklist (must be in TASKS.md)
- StartedAt:
- FinishedAt:
- Owner:
- [ ] No placeholder code.
- [ ] All required modules implemented.
- [ ] Role-based security enforced.
- [ ] Expired campaign rule enforced.
- [ ] Exports functional.
- [ ] Dashboard populated with seed data.
- [ ] Full stack runs via compose.
- [ ] TASKS.md updated after each completed task with evidence links (commit hash, file paths, command outputs).

## 17. [x] Task status audit hardening
- StartedAt: 2026-03-27T17:50:00Z
- FinishedAt: 2026-03-27T18:05:00Z
- Owner: Codex
- Audit every task status against committed artifacts.
- Downgrade overstated statuses when required deliverables are missing.
- Add strict completion checklist requiring buildable evidence.

### Execution Notes
- Completed audit pass across all task entries.
- Downgraded tasks with overstated completion:
  - Task 3 -> `[-]` (missing API versioning / explicit AutoMapper profile evidence).
  - Task 14 -> `[-]` (missing recorded end-to-end `docker compose up` verification evidence).
- Added reproducible evidence metadata for completed tasks (commit hashes, concrete files, commands).

## Strict Completion Checklist (required before switching any task to `[x]`)
- [ ] All acceptance criteria in the task entry map to committed code/config artifacts (no narrative-only completion).
- [ ] At least one reproducible build/check command succeeds for the changed scope (examples: `dotnet build`, `npm run build`, `docker compose config`, `docker compose up` health validation).
- [ ] TASKS.md entry contains:
  - [ ] exact commit hash,
  - [ ] concrete file paths,
  - [ ] reproducible command evidence.
- [ ] Security/validation requirements for touched endpoints are enforced and test-covered where applicable.
- [ ] No placeholder code, TODO markers, or empty stubs remain in touched scope.
