# TASKS

Status legend:
- [ ] Pending
- [-] In Progress
- [x] Completed

## Phase 1 - Repository Reset & Bootstrap

- [x] Confirm reset policy (preserve `.git` and control docs only).
- [x] Remove legacy repository contents.
- [x] Recreate backend-first folder topology (API, Application, Domain, Infrastructure, Gateway, Worker, Frontend, Docs, Scripts).
- [x] Add baseline repository files (`README.md`, `.gitignore`, placeholders).
- [x] Validate directory structure and capture evidence in task notes/commit history.

## Phase 2 - Proxy Generation System

- [ ] Define OpenAPI source-of-truth boundaries per backend service.
- [ ] Add proxy generation configuration (tooling, output folders, naming standards).
- [ ] Create generation command scripts for local/dev CI usage.
- [ ] Add contract drift validation (fail when proxies are stale vs Swagger).
- [ ] Add developer docs for regeneration workflow and troubleshooting.

## Phase 3 - Setup Wizard Architecture

- [ ] Define setup domain concepts (setup state, completion rules, invariants).
- [ ] Define application contracts/commands/queries for setup flow.
- [ ] Define API surface for setup-only endpoints and bootstrap checks.
- [ ] Define frontend route-guard and redirect architecture (block app until setup complete).
- [ ] Define end-to-end setup sequence diagram (API + Gateway + UI integration path).

## Phase 4 - Setup Backend APIs

- [ ] Implement setup aggregate/entity and required value objects.
- [ ] Implement infrastructure configuration/repository for setup state.
- [ ] Implement setup commands/queries and MediatR handlers.
- [ ] Implement FluentValidation validators for setup requests.
- [ ] Expose setup endpoints (ProblemDetails + auth scope rules where applicable).
- [ ] Expose setup Swagger contracts and verify proxy generation output.

## Phase 5 - Runtime Configuration Persistence

- [ ] Define runtime configuration model and secure storage requirements.
- [ ] Implement configuration persistence abstraction + infrastructure provider.
- [ ] Implement write/update/read commands and handlers.
- [ ] Implement validation and normalization rules for configuration input.
- [ ] Add operational endpoints/health checks for config readiness.
- [ ] Document secret handling, rotation points, and startup load behavior.

## Phase 6 - Backend Foundation

- [ ] Create shared domain primitives (base entity/value object/enums/events as needed).
- [ ] Create application cross-cutting behaviors (validation, logging, authorization pipeline).
- [ ] Register dependency injection modules per layer without boundary leaks.
- [ ] Implement base repository/unit-of-work patterns for write workflows.
- [ ] Add global exception handling + ProblemDetails mapping.
- [ ] Add foundational unit tests for cross-cutting behaviors.

## Phase 7 - Swagger Standards

- [ ] Define OpenAPI conventions (grouping, tags, versioning, auth schemes).
- [ ] Implement Swagger configuration in API host.
- [ ] Enforce schema quality (operation IDs, examples, response contracts).
- [ ] Add CI/API checks to ensure Swagger document generation succeeds.
- [ ] Document Swagger usage for backend/frontend/proxy workflows.

## Phase 8 - Gateway (Ocelot)

- [ ] Define gateway route map for all backend modules.
- [ ] Implement Ocelot route configuration and upstream/downstream policies.
- [ ] Implement auth forwarding, claims propagation, and role policy mapping.
- [ ] Implement gateway middleware for correlation/logging/error shaping.
- [ ] Add gateway health/readiness routes and smoke tests.

## Phase 9 - Identity (Keycloak)

- [ ] Define realm/client/role mapping requirements (`Admin`, `Operator`, `Viewer`).
- [ ] Implement JWT bearer authentication integration in API and Gateway.
- [ ] Implement authorization policies mapped to module actions.
- [ ] Implement identity configuration binding and startup validation checks.
- [ ] Document local/dev identity bootstrap and credential rotation workflow.

## Phase 10 - Database & Migrations

- [ ] Define initial relational schema boundaries per module.
- [ ] Implement EF Core DbContext(s), mappings, and indexes.
- [ ] Create initial migration set and migration execution scripts.
- [ ] Implement transaction strategy and unit-of-work consistency boundaries.
- [ ] Add database health checks and startup migration strategy policy.
- [ ] Add integration tests for persistence-critical paths.

## Phase 11 - Campaign Module

- [ ] Define campaign entities/value objects and domain rules.
- [ ] Implement campaign write model repositories/configuration.
- [ ] Implement commands/queries + handlers (create/update/list/detail/lifecycle actions).
- [ ] Implement validators for campaign DTOs and filters.
- [ ] Expose campaign endpoints with authorization + ProblemDetails responses.
- [ ] Verify campaign Swagger contracts and regenerate proxies.
- [ ] Connect frontend campaign features using generated proxies only.

## Phase 12 - Template Module

- [ ] Define template entities (content, metadata, version constraints).
- [ ] Implement template persistence configuration and repositories.
- [ ] Implement template commands/queries + handlers.
- [ ] Implement template validators (content safety, required fields, constraints).
- [ ] Expose template endpoints and response contracts.
- [ ] Verify Swagger + regenerate template proxies.
- [ ] Connect frontend template flows through generated proxies.

## Phase 13 - Tracking Module

- [ ] Define tracking domain model (events, status transitions, correlation IDs).
- [ ] Implement tracking storage configuration and optimized query indexes.
- [ ] Implement tracking ingestion/query handlers with cancellation support.
- [ ] Implement validators for tracking ingest/query requests.
- [ ] Expose tracking endpoints and event retrieval contracts.
- [ ] Verify tracking Swagger + proxy regeneration.
- [ ] Connect frontend tracking views to generated proxies.

## Phase 14 - Background Jobs

- [ ] Define job types, retry policies, and idempotency rules.
- [ ] Implement worker infrastructure (queue scheduling, execution pipeline).
- [ ] Implement job command dispatch handlers and background processors.
- [ ] Implement validation/guardrails for job payloads and scheduling inputs.
- [ ] Expose operational endpoints for job status where required.
- [ ] Add observability (structured logs, metrics, failure alerts) for jobs.

## Phase 15 - Analytics

- [ ] Define analytics domain metrics and aggregation boundaries.
- [ ] Implement read-optimized analytics projections/storage strategy.
- [ ] Implement analytics queries/handlers and aggregation services.
- [ ] Implement query validators (range, granularity, authorization scope).
- [ ] Expose analytics endpoints and dashboard-ready contracts.
- [ ] Verify Swagger + regenerate analytics proxies.
- [ ] Connect frontend analytics dashboards to generated proxies (no mock KPI data).

## Phase 16 - Export System

- [ ] Define export domain model (request, format, lifecycle, audit fields).
- [ ] Implement export storage/infrastructure (file provider, retention policy).
- [ ] Implement export commands/handlers (request generation, retrieval, cleanup).
- [ ] Implement validators for export parameters and authorization context.
- [ ] Expose export endpoints (request status/download) with secure access.
- [ ] Verify Swagger + regenerate export proxies.
- [ ] Connect frontend export flows through generated proxies.

## Phase 17 - Frontend (Generated Proxies Only)

- [ ] Verify all required backend endpoints exist and appear in Swagger before UI work.
- [ ] Regenerate all frontend proxies and commit generated artifacts.
- [ ] Implement feature modules using generated proxies only (no handwritten duplicate services).
- [ ] Implement route guards, setup redirect rules, and role-based page access.
- [ ] Implement reactive form validation aligned with backend validators.
- [ ] Implement error/loading/success UX bound to real API responses.
- [ ] Run frontend compile checks and contract integration tests.

## Phase 18 - Build / Publish Scripts

- [ ] Define build matrix (API, Gateway, Worker, Frontend) for local and CI.
- [ ] Implement reproducible build scripts (restore/build/test/package).
- [ ] Implement publish scripts/artifact output conventions.
- [ ] Implement environment configuration templates and validation commands.
- [ ] Add CI pipeline stages and quality gates (tests, lint, swagger/proxy freshness).
- [ ] Document release execution sequence and rollback notes.

## Phase 19 - Final Validation

- [ ] Execute full backend build and test suite.
- [ ] Execute gateway integration smoke tests.
- [ ] Execute frontend build and end-to-end sanity checks.
- [ ] Verify Swagger documents and proxy freshness across modules.
- [ ] Verify setup wizard gating and runtime configuration persistence behavior.
- [ ] Verify security baseline (JWT auth, role authorization, input validation, ProblemDetails).
- [ ] Remove residual placeholders/dead code and finalize production-readiness checklist.

## Definition of Done

- [ ] Backend build passes (attach command + timestamp + result, e.g., `dotnet build` logs in task evidence).
- [ ] Runtime behavior verified (record executed runtime checks, scenarios, and observed outcomes).
- [ ] Swagger reflects contract changes (capture generated OpenAPI diff/snapshot and endpoint verification evidence).
- [ ] Proxies regenerated and synced (record regeneration command, changed files, and zero-drift validation result).
- [ ] Frontend compile passes (attach compile command output, e.g., `npm run build`/`ng build`, with timestamp).
- [ ] No placeholder/fake data remains (document audit of removed mocks/stubs and impacted files).
- [ ] `TASKS.md` updated with evidence (link each completed task to concrete proof: commands, logs, screenshots, or artifacts).
- [ ] Security and role rules verified for protected endpoints (evidence of JWT auth + `Admin`/`Operator`/`Viewer` authorization checks).

## Notes

- 2026-04-17: Verified the top-level directories documented in `README.md` already exist with matching names: `backend/`, `gateway/`, `worker/`, `frontend/`, `docs/`, and `scripts/`. No naming changes were required.
