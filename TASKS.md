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

### Subtasks (planned)
- [x] **Enforce strict layer dependency graph**
  - **Description:** Verify project references satisfy Clean Architecture (`Domain <- Application <- Infrastructure <- API`) and remove any reverse coupling.
  - **Expected output:** Buildable solution with one-way dependency flow and no cross-layer leakage.
  - **Related layer:** Backend
- [x] **Complete API versioning implementation**
  - **Description:** Configure API versioning, default version assumptions, and versioned route conventions.
  - **Expected output:** Versioned endpoints with deterministic routing and discoverable version behavior.
  - **Related layer:** Backend
- [x] **Add explicit AutoMapper profile coverage**
  - **Description:** Create mapping profiles per feature and validate critical mappings used by CQRS handlers.
  - **Expected output:** Registered mapping profiles with compile-safe mappings for command/query DTOs.
  - **Related layer:** Backend
- [x] **Harden cross-cutting request pipeline**
  - **Description:** Ensure FluentValidation and ProblemDetails are consistently applied across all CQRS endpoints.
  - **Expected output:** Uniform validation failures and error contracts without controller business logic.
  - **Related layer:** Backend
- [x] **Operationalize health and observability baseline**
  - **Description:** Expand health checks/readiness and structured log enrichment for traceability in distributed runtime.
  - **Expected output:** Production-ready health endpoints and enriched structured logs for API diagnostics.
  - **Related layer:** Infra

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
- Added architecture dependency graph guard test in `API.IntegrationTests` to enforce strict project references:
  - `Domain` has no project references.
  - `Application` references only `Domain`.
  - `Infrastructure` references only `Domain` and `Application`.
  - `API` references only `Application` and `Infrastructure`.
- Audit adjustment:
  - Downgraded to `[-] in progress` because API versioning and explicit AutoMapper profiles are not implemented yet, so completion criteria are not fully met.
  - Reproducible command evidence:
    - `rg -n "AddApiVersioning|ApiVersion" backend/API backend/Application`
    - `rg -n "Profile\\b|CreateMap\\(" backend/Application`
- Subtask completion update (2026-03-27):
  - Implemented API versioning with default `1.0`, `AssumeDefaultVersionWhenUnspecified`, combined readers (URL segment + `x-api-version` header + `api-version` query), and API version reporting.
  - Applied versioned route conventions on controllers with explicit `[ApiVersion(\"1.0\")]` and URL-segment routes (`/api/v{version}/...`) while preserving unversioned compatibility routes.
  - Added integration coverage for versioned access endpoint and discoverable version behavior via `api-supported-versions` response header.
  - Reproducible command evidence:
    - `rg -n "AddApiVersioning|DefaultApiVersion|AssumeDefaultVersionWhenUnspecified|ReportApiVersions|ApiVersionReader" backend/API/Program.cs`
    - `rg -n "ApiVersion|v\\{version:apiVersion\\}" backend/API/Controllers/HealthController.cs backend/API/Controllers/AccessController.cs`
    - `rg -n "versioned|api-supported-versions|/api/v1/access/admin" backend/API.IntegrationTests/AuthorizationFlowTests.cs`

- Subtask completion update (2026-03-27):
  - Added explicit per-feature AutoMapper profile `HealthMappingProfile` for CQRS response mapping (`HealthStatus` -> `HealthStatusDto`).
  - Updated `GetHealthQueryHandler` to map through `IMapper` instead of inline DTO construction.
  - Added integration-level mapping validation test to assert global mapping configuration and feature mapping behavior.
  - Reproducible command evidence:
    - `rg -n "AddAutoMapper|HealthMappingProfile|CreateMap<HealthStatus, HealthStatusDto>" backend/Application`
    - `rg -n "IMapper|mapper.Map<HealthStatusDto>" backend/Application/Features/Health/GetHealthQueryHandler.cs`
    - `dotnet test backend/API.IntegrationTests/API.IntegrationTests.csproj`

- Subtask completion update (2026-03-27):
  - Added centralized API exception handling with a dedicated `GlobalExceptionHandler` that maps FluentValidation `ValidationException` to RFC7807 `HttpValidationProblemDetails` and returns consistent 400 responses.
  - Registered exception handling pipeline via `AddExceptionHandler<GlobalExceptionHandler>()` and enriched all ProblemDetails payloads with `traceId` for diagnostics.
  - Added integration test coverage that injects a failing validator for a CQRS request and asserts deterministic `application/problem+json` validation response contract.
  - Reproducible command evidence:
    - `rg -n "GlobalExceptionHandler|AddExceptionHandler|AddProblemDetails\\(" backend/API/Program.cs backend/API/ExceptionHandling/GlobalExceptionHandler.cs`
    - `rg -n "ValidationPipelineTests|AlwaysFailGetHealthQueryValidator|application/problem\\+json" backend/API.IntegrationTests/ValidationPipelineTests.cs`
    - `dotnet test backend/API.IntegrationTests/API.IntegrationTests.csproj`

- Subtask completion update (2026-03-27):
  - Expanded health diagnostics with explicit liveness/readiness endpoints:
    - `/health` (full),
    - `/health/live` (process liveness),
    - `/health/ready` (readiness-tagged dependencies).
  - Tagged infrastructure configuration health check as readiness-only and kept health endpoints anonymous for orchestrator probes.
  - Added structured log enrichment for distributed diagnostics (`TraceId`, `CorrelationId`, request/user/network metadata) and propagated `X-Correlation-ID` on responses.
  - Added integration coverage validating anonymous health endpoints, JSON response contract, and correlation ID propagation.
  - Reproducible command evidence:
    - `rg -n "MapHealthChecks\\(\"/health|/health/live|/health/ready|WriteHealthResponseAsync|UseSerilogRequestLogging\\(options|X-Correlation-ID" backend/API/Program.cs`
    - `rg -n "AddCheck<InfrastructureOptionsHealthCheck>|tags: \\[\"ready\"\\]" backend/Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`
    - `rg -n "Health_Endpoints_Should_Be_Anonymous_And_Return_Json|/health/live|/health/ready|X-Correlation-ID" backend/API.IntegrationTests/HealthChecksEndpointsTests.cs`
    - `dotnet test backend/API.IntegrationTests/API.IntegrationTests.csproj`


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
- [x] **Define campaign aggregate and value rules**
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

### Execution Notes
- Subtask completion update (2026-03-27):
  - Added a pure Domain campaign aggregate with explicit invariants for required `Name`, non-empty `HtmlContent`, and `StartDate <= EndDate`.
  - Added campaign lifecycle enum and transition guard enforcing allowed flow `Draft -> Scheduled -> Active -> Ended/Archived`.
  - Added domain-specific exceptions and a status-change domain event contract for downstream application handling.
  - Reproducible command evidence:
    - `rg -n "class Campaign|AllowedStatusTransitions|ValidateDateRange|ChangeStatus" backend/Domain/Campaigns/Campaign.cs`
    - `rg -n "enum CampaignStatus|enum TemplateType|CampaignValidationException|InvalidCampaignStatusTransitionException|CampaignStatusChangedDomainEvent" backend/Domain/Campaigns`
    - `dotnet build backend/QPhising.Backend.sln` *(fails in current environment: `dotnet` not installed)*

## 6. [ ] Template module
- StartedAt:
- FinishedAt:
- Owner:
- Implement email template builder backend APIs and storage schema.
- Implement landing page renderer pipeline with safe HTML handling and variable substitution.

### Subtasks (planned)
- [ ] **Model template domain and variable contracts**
  - **Description:** Define template aggregate, supported template types, variable placeholder format, and lifecycle states.
  - **Expected output:** Domain models and invariants for template correctness and reuse.
  - **Related layer:** Backend
- [ ] **Implement template CQRS workflows**
  - **Description:** Add create/update/get/list/publish/archive commands and queries through MediatR with FluentValidation.
  - **Expected output:** End-to-end application handlers for template lifecycle operations.
  - **Related layer:** Backend
- [ ] **Build safe HTML sanitization pipeline**
  - **Description:** Apply allowlist-based sanitization before persistence/rendering and reject unsafe markup patterns.
  - **Expected output:** Sanitized template content that prevents unsafe script/style injection vectors.
  - **Related layer:** Backend
- [ ] **Implement variable substitution engine**
  - **Description:** Resolve template placeholders from approved data sources with deterministic missing-variable behavior.
  - **Expected output:** Renderer service producing final HTML with validated substitutions.
  - **Related layer:** Backend
- [ ] **Add template persistence schema and indexes**
  - **Description:** Configure infrastructure persistence, versioning constraints, and migrations for template storage.
  - **Expected output:** Migration-ready schema with efficient query/index strategy.
  - **Related layer:** Infra
- [ ] **Expose secured template endpoints**
  - **Description:** Add thin API controllers delegating to CQRS with JWT/RBAC enforcement and ProblemDetails responses.
  - **Expected output:** Role-protected template API surface with stable request/response contracts.
  - **Related layer:** Backend

## 7. [ ] Tracking module
- StartedAt:
- FinishedAt:
- Owner:
- Generate unique tracking links with signed tokens.
- Log click metadata (IP, UserAgent, Timestamp, Fingerprint).
- Use Redis for deduplication and real-time counters.
- Define replay/abuse protections and retention policy.

### Subtasks (planned)
- [ ] **Define signed tracking token specification**
  - **Description:** Specify token payload, signature method, expiration semantics, and validation flow.
  - **Expected output:** Deterministic token contract for tamper-resistant tracking URLs.
  - **Related layer:** Backend
- [ ] **Implement tracking link generation CQRS**
  - **Description:** Add command handler to issue unique tracking links bound to campaign and recipient context.
  - **Expected output:** Unique, signed tracking links generated via application layer.
  - **Related layer:** Backend
- [ ] **Implement click ingestion and metadata persistence**
  - **Description:** Add endpoint/handler to validate tokens and persist click metadata with normalized schema.
  - **Expected output:** Reliable click event records including IP, UserAgent, Timestamp, and Fingerprint.
  - **Related layer:** Backend
- [ ] **Add Redis deduplication and realtime counters**
  - **Description:** Use Redis keys for dedup windows and atomic increments for campaign/recipient click counters.
  - **Expected output:** Idempotent click handling and low-latency metrics updates.
  - **Related layer:** Infra
- [ ] **Implement replay and abuse protections**
  - **Description:** Add nonce/time-window checks, suspicious-rate thresholds, and reject/flag behavior.
  - **Expected output:** Hardened tracking pipeline against replay and abusive click traffic.
  - **Related layer:** Backend
- [ ] **Define retention and archival policy implementation**
  - **Description:** Set lifecycle rules for raw click data, aggregates, and cleanup scheduling.
  - **Expected output:** Enforced retention policy with documented operational behavior.
  - **Related layer:** Infra

## 8. [-] Task execution engine
- StartedAt: 2026-03-27T16:52:30Z
- FinishedAt:
- Owner: Codex
- Implement Task entity (`Id`, `Type`, `Status`, `Payload`, `Logs`, `CreatedAt`).
- Build queueing and processing with Hangfire or Worker Service.
- Add retry policy, dead-letter handling, and execution logs.

### Subtasks (planned)
- [ ] **Formalize task aggregate and transition rules**
  - **Description:** Define `Task` lifecycle transitions, task-type contracts, payload schema, and execution status semantics.
  - **Expected output:** Domain-consistent task model and transition guards suitable for queue processing.
  - **Related layer:** Backend
- [ ] **Implement durable queue persistence strategy**
  - **Description:** Persist queued tasks with claim/lease semantics and concurrency-safe status updates.
  - **Expected output:** Restart-safe queue storage with deterministic worker claim behavior.
  - **Related layer:** Infra
- [ ] **Implement worker dispatcher and handler registry**
  - **Description:** Route task types to dedicated handlers and enforce standardized execution contract.
  - **Expected output:** Extensible execution pipeline for background tasks with consistent handler outcomes.
  - **Related layer:** Backend
- [ ] **Add retry/backoff and dead-letter handling**
  - **Description:** Introduce retry policy per task type with capped attempts and dead-letter terminal state.
  - **Expected output:** Predictable failure recovery and dead-lettered task visibility.
  - **Related layer:** Backend
- [ ] **Add execution logging and observability**
  - **Description:** Persist structured execution logs, correlation IDs, and timing metrics for each task run.
  - **Expected output:** Queryable execution history and diagnostics for operations and troubleshooting.
  - **Related layer:** Infra

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

### Subtasks (planned)
- [ ] **Define KPI contracts and filter dimensions**
  - **Description:** Specify response models and filters for campaign, click, conversion, and throughput metrics.
  - **Expected output:** Stable analytics contracts consumable by dashboard components.
  - **Related layer:** Backend
- [ ] **Implement analytics CQRS query handlers**
  - **Description:** Add efficient aggregation queries for totals, trends, and grouped breakdowns.
  - **Expected output:** Performant analytics read handlers with deterministic pagination/time-window behavior.
  - **Related layer:** Backend
- [ ] **Integrate Redis caching and invalidation**
  - **Description:** Define cache keys/TTL and invalidate or refresh cached KPI data on relevant write events.
  - **Expected output:** Reduced analytics latency with controlled consistency guarantees.
  - **Related layer:** Infra
- [ ] **Implement realtime analytics update channel**
  - **Description:** Add SignalR (or equivalent) hub and publish KPI update events with authorization checks.
  - **Expected output:** Live dashboard updates for subscribed authenticated clients.
  - **Related layer:** Backend
- [ ] **Expose secured dashboard endpoints**
  - **Description:** Add API endpoints for analytics widgets with role-based access enforcement.
  - **Expected output:** JWT/RBAC-protected analytics API surface ready for frontend integration.
  - **Related layer:** Backend

## 10. [ ] Export subsystem
- StartedAt:
- FinishedAt:
- Owner:
- Implement Excel exports via ClosedXML.
- Implement PDF exports (campaign and analytics reports).
- Add async export task processing and download endpoints.

### Subtasks (planned)
- [ ] **Define export job lifecycle model**
  - **Description:** Model export request, format, status progression, ownership, and file metadata.
  - **Expected output:** Consistent domain/application contract for asynchronous exports.
  - **Related layer:** Backend
- [ ] **Implement Excel export pipeline**
  - **Description:** Build ClosedXML-based service for campaign and analytics datasets with enterprise-ready formatting.
  - **Expected output:** Reproducible `.xlsx` outputs aligned with report requirements.
  - **Related layer:** Backend
- [ ] **Implement PDF report generation**
  - **Description:** Build PDF rendering service for campaign and analytics report layouts.
  - **Expected output:** Downloadable `.pdf` reports generated from validated export payloads.
  - **Related layer:** Backend
- [ ] **Integrate async export processing**
  - **Description:** Queue export jobs to background worker with retry/dead-letter support and status updates.
  - **Expected output:** Non-blocking export execution with reliable job tracking.
  - **Related layer:** Backend
- [ ] **Provide secured export APIs and file delivery**
  - **Description:** Add create/status/download endpoints with JWT/RBAC and ownership checks.
  - **Expected output:** Secure, auditable export workflow from request to file retrieval.
  - **Related layer:** Backend
- [ ] **Implement export file retention cleanup**
  - **Description:** Define storage TTL and cleanup process for generated export files.
  - **Expected output:** Controlled storage growth and compliance-ready retention behavior.
  - **Related layer:** Infra

## 11. [-] Frontend enterprise UI
- StartedAt: 2026-03-27T16:40:00Z
- FinishedAt:
- Owner: Codex
- Initialize Angular latest with PrimeNG + Tailwind + ApexCharts.
- Implement feature-based architecture with smart/dumb components.
- Add state management (NgRx or Signals), routing guards, role-based menus.
- Deliver premium SaaS layout: sidebar, KPI cards, charts, filterable tables, dark mode, responsive behavior.

### Subtasks (planned)
- [ ] **Establish feature-based frontend module map**
  - **Description:** Define modules for dashboard, campaigns, templates, tracking, tasks, analytics, and exports with smart/dumb separation.
  - **Expected output:** Scalable Angular feature structure and reusable shared UI components.
  - **Related layer:** Frontend
- [ ] **Implement global state strategy**
  - **Description:** Introduce NgRx or Signals for auth/session, dashboard data, and feature-level view state.
  - **Expected output:** Predictable frontend state flows with typed selectors/actions.
  - **Related layer:** Frontend
- [ ] **Implement route guards and role-aware navigation**
  - **Description:** Add authentication guards and role-based menu rendering/route access checks.
  - **Expected output:** Protected frontend routes and consistent RBAC-driven navigation UX.
  - **Related layer:** Frontend
- [ ] **Build enterprise dashboard experience**
  - **Description:** Implement KPI cards, chart panels, filterable data tables, and responsive layout using PrimeNG + Tailwind.
  - **Expected output:** Production-grade SaaS dashboard UI with coherent spacing and component consistency.
  - **Related layer:** Frontend
- [ ] **Add dark mode and accessibility polish**
  - **Description:** Implement theming, keyboard accessibility, loading/empty/error states, and UX consistency checks.
  - **Expected output:** Accessible, polished UI behavior across desktop/tablet/mobile breakpoints.
  - **Related layer:** Frontend

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

### Subtasks (planned)
- [ ] **Complete gateway route matrix**
  - **Description:** Enumerate and configure Ocelot routes for all service endpoint families with explicit upstream/downstream mappings.
  - **Expected output:** Comprehensive route coverage with deterministic forwarding rules.
  - **Related layer:** Infra
- [ ] **Implement distributed Redis rate limiting**
  - **Description:** Wire Redis-backed rate-limiting strategy for consistent throttling across gateway instances.
  - **Expected output:** Cluster-safe throttling with standardized 429 responses and headers.
  - **Related layer:** Infra
- [ ] **Implement correlation ID propagation**
  - **Description:** Generate/forward correlation IDs from gateway to downstream APIs and include them in logs.
  - **Expected output:** End-to-end request traceability across gateway and backend logs.
  - **Related layer:** Infra
- [ ] **Harden structured access logging**
  - **Description:** Enrich logs with route, principal, status code, latency, and throttling/security outcomes.
  - **Expected output:** Operationally useful access/audit logs for incident analysis.
  - **Related layer:** Infra
- [ ] **Validate gateway auth/rate-limit behavior**
  - **Description:** Add integration-level checks for JWT policy enforcement and throttling/correlation requirements.
  - **Expected output:** Reproducible verification evidence for gateway security and resilience behavior.
  - **Related layer:** Backend

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

### Subtasks (planned)
- [ ] **Finalize relational schema for all active modules**
  - **Description:** Define entities, relationships, constraints, and indexes across campaign/template/tracking/task/analytics/export domains.
  - **Expected output:** Coherent normalized schema aligned with application use cases.
  - **Related layer:** Infra
- [ ] **Create baseline and incremental migrations**
  - **Description:** Generate EF migrations for current model and validate upgrade/downgrade behavior.
  - **Expected output:** Reproducible migration chain for local and containerized deployment.
  - **Related layer:** Infra
- [ ] **Implement identity and role seed strategy**
  - **Description:** Seed role assumptions and default users metadata needed for RBAC-enabled environments.
  - **Expected output:** Deterministic bootstrap data supporting secure role-based flows.
  - **Related layer:** Infra
- [ ] **Seed representative business and analytics data**
  - **Description:** Seed campaigns, templates, task history, and KPI bootstrap records for realistic UI/API validation.
  - **Expected output:** Non-placeholder seed dataset suitable for dashboard and export verification.
  - **Related layer:** Infra
- [ ] **Ensure idempotent seed execution**
  - **Description:** Implement re-runnable seed scripts/initializers that avoid duplicate records and preserve integrity.
  - **Expected output:** Safe repeated seed runs across developer and CI environments.
  - **Related layer:** Infra

## 14. [-] Docker and runtime
- StartedAt: 2026-03-27T16:55:00Z
- FinishedAt: 2026-03-27T17:20:00Z
- Owner: Codex
- Add Dockerfiles for `frontend`, `api`, `gateway`.
- Build `docker-compose.yml` with `frontend`, `api`, `gateway`, `postgres`, `keycloak`, `redis`.
- Verify one-command startup path (`docker-compose up`) and initialization order.

### Subtasks (planned)
- [ ] **Validate deterministic image builds**
  - **Description:** Confirm Dockerfiles produce reproducible artifacts for frontend, API, gateway, and worker.
  - **Expected output:** Consistent container images with documented build assumptions.
  - **Related layer:** Infra
- [ ] **Finalize compose startup dependencies**
  - **Description:** Align healthchecks and `depends_on` conditions with true service readiness.
  - **Expected output:** Reliable startup ordering with minimized race conditions.
  - **Related layer:** Infra
- [ ] **Verify full-stack `docker compose up` lifecycle**
  - **Description:** Execute and document one-command startup, health convergence, and service reachability checks.
  - **Expected output:** Recorded evidence of healthy end-to-end stack initialization.
  - **Related layer:** Infra
- [ ] **Audit environment variable to config mapping**
  - **Description:** Ensure all runtime settings are environment-driven and mapped to application configuration keys.
  - **Expected output:** No hidden hardcoded runtime values; portable runtime configuration.
  - **Related layer:** Infra
- [ ] **Document compose operational profiles**
  - **Description:** Define local/deployment compose usage patterns and troubleshooting notes.
  - **Expected output:** Clear runtime documentation for developers and operators.
  - **Related layer:** Infra

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

### Subtasks (planned)
- [ ] **Define multi-stage CI workflow**
  - **Description:** Configure CI stages for restore/build/lint/test/security scan/container build across backend and frontend.
  - **Expected output:** End-to-end automated pipeline with explicit stage dependencies.
  - **Related layer:** Infra
- [ ] **Implement quality thresholds and merge gates**
  - **Description:** Add lint/test/coverage thresholds and fail conditions to block low-quality merges.
  - **Expected output:** Enforced quality gates with transparent pass/fail criteria.
  - **Related layer:** Infra
- [ ] **Integrate dependency and container vulnerability scanning**
  - **Description:** Add SCA and image scanning with policy-based severity thresholds.
  - **Expected output:** Automated vulnerability reporting with actionable blocking rules.
  - **Related layer:** Infra
- [ ] **Add secret/config security checks**
  - **Description:** Integrate secret detection and insecure configuration policy checks into CI.
  - **Expected output:** Early detection of credential leaks and risky runtime settings.
  - **Related layer:** Infra
- [ ] **Produce operational runbooks**
  - **Description:** Document backup/restore procedures, incident response workflow, and logging/observability guidance.
  - **Expected output:** Practical operational documentation for support and on-call execution.
  - **Related layer:** Infra

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

### Subtasks (planned)
- [ ] **Translate each DoD line into measurable acceptance criteria**
  - **Description:** Define exact evidence required for each checklist item (artifact + command + expected result).
  - **Expected output:** Objective, auditable DoD criteria that can be validated consistently.
  - **Related layer:** Infra
- [ ] **Map DoD criteria to owning delivery tasks**
  - **Description:** Create traceability between DoD checklist items and specific task IDs/modules.
  - **Expected output:** Clear ownership matrix preventing ambiguous completion claims.
  - **Related layer:** Infra
- [ ] **Standardize completion evidence template**
  - **Description:** Enforce a uniform per-task evidence block format (commit hash, files, reproducible commands).
  - **Expected output:** Consistent completion documentation across all task entries.
  - **Related layer:** Infra
- [ ] **Add release-readiness final verification pass**
  - **Description:** Define a final pre-release review gate that confirms all DoD criteria are satisfied.
  - **Expected output:** Single, repeatable release-go/no-go verification checkpoint.
  - **Related layer:** Infra

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
