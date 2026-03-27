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
- Evidence:
  - Commit: `41f4116`
  - File: `TASKS.md`
  - Commands: `cat > TASKS.md ...`

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
- Evidence:
  - Commit: `41f4116`
  - File paths listed above.
  - Commands:
    - `mkdir -p frontend backend Domain Application Infrastructure API gateway docker`
    - `cat > .editorconfig ...`
    - `cat > .gitignore ...`
    - `cat > .prettierrc.json ...`
    - `cat > .eslintrc.json ...`
    - `cat > QPhising.code-workspace ...`

---

## 3. [-] Backend Clean Architecture foundation
- StartedAt: 2026-03-27T16:13:00Z
- FinishedAt:
- Owner: Codex
- Initialize .NET 10 solution with projects for `Domain`, `Application`, `Infrastructure`, `API`.
- Add CQRS stack (MediatR), FluentValidation pipeline behaviors, AutoMapper profiles, repository + unit-of-work contracts.
- Add API versioning, problem details, structured logging, health checks.

### Execution Notes
- Attempted prerequisite tool check: `dotnet --version`.
- Current environment is missing .NET SDK (`dotnet: command not found`), so implementation is blocked until SDK/runtime is installed.

---

## 4. [ ] Identity and authorization
- StartedAt:
- FinishedAt:
- Owner:
- Integrate Keycloak JWT validation in API and gateway.
- Define role policies: `Admin`, `Operator`, `Viewer`.
- Add authorization matrix per endpoint family in TASKS.md.

## 5. [ ] Campaign management module
- StartedAt:
- FinishedAt:
- Owner:
- Implement domain model: Campaign (Name, TemplateType, HtmlContent, StartDate, EndDate, Status).
- Add commands/queries for CRUD, activation, scheduling.
- Enforce rule: expired campaigns reject tracking link generation and click processing.

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

## 8. [ ] Task execution engine
- StartedAt:
- FinishedAt:
- Owner:
- Implement Task entity (`Id`, `Type`, `Status`, `Payload`, `Logs`, `CreatedAt`).
- Build queueing and processing with Hangfire or Worker Service.
- Add retry policy, dead-letter handling, and execution logs.

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

## 11. [ ] Frontend enterprise UI
- StartedAt:
- FinishedAt:
- Owner:
- Initialize Angular latest with PrimeNG + Tailwind + ApexCharts.
- Implement feature-based architecture with smart/dumb components.
- Add state management (NgRx or Signals), routing guards, role-based menus.
- Deliver premium SaaS layout: sidebar, KPI cards, charts, filterable tables, dark mode, responsive behavior.

## 12. [ ] Gateway implementation
- StartedAt:
- FinishedAt:
- Owner:
- Configure Ocelot routes to API services.
- Enforce JWT validation, Redis-backed rate limiting, correlation IDs, and access logging.

## 13. [ ] Data, migrations, and seed
- StartedAt:
- FinishedAt:
- Owner:
- Configure PostgreSQL migrations.
- Add seed data: users/roles mapping assumptions, sample campaigns, templates, task history, analytics bootstrap data.

## 14. [ ] Docker and runtime
- StartedAt:
- FinishedAt:
- Owner:
- Add Dockerfiles for `frontend`, `api`, `gateway`.
- Build `docker-compose.yml` with `frontend`, `api`, `gateway`, `postgres`, `keycloak`, `redis`.
- Verify one-command startup path (`docker-compose up`) and initialization order.

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
