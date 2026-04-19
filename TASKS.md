# TASKS

Status legend:
- [ ] Pending
- [-] In Progress
- [x] Completed

Execution rules:
- Only one task may be marked `[-] In Progress` at a time.
- Complete tasks in dependency order unless an explicit blocker requires reordering.
- Record concise implementation evidence in commit messages and PR notes.

## Phase 0 - Cleanup & Refactor
- [x] Audit repository for legacy phishing/email/mail concepts across Domain, Application, Infrastructure, API, Gateway, and Frontend.
- [x] Remove or rename legacy phishing/email entities, value objects, enums, services, controllers, DTOs, and UI labels.
- [x] Remove obsolete routes/endpoints and deprecated proxy surfaces tied to phishing/email workflows.
- [x] Inspect package and project dependencies; remove unused libraries and stale references.
- [x] Update naming conventions and module boundaries to align with Web Page Tracking & Visitor Analytics terminology.

Phase 0 evidence:
- 2026-04-19: Removed campaign target email workflow across Domain/Application/API/Frontend, updated proxy contract guards, and refreshed proxy validation OpenAPI artifact to remove obsolete target surfaces.

## Phase 1 - Domain & Database
- [x] Define `TrackingPage` aggregate (identity, slug, metadata, publish state, ownership).
- [x] Define `VisitEvent` aggregate/entity (timestamp, page reference, visitor fingerprint/session, user-agent, referrer, IP-hash policy).
- [x] Define optional `PageSettings` model (retention, privacy options, bot filtering, UTM capture toggles).
- [x] Add domain invariants and value objects for slugs, URLs, and tracking identifiers.
- [x] Implement EF Core entity configurations for tracking models.
- [x] Add database migration(s) for tracking pages, visit events, and supporting indexes.
- [x] Validate migration rollback/forward strategy for local and CI environments.

Phase 1 evidence:
- 2026-04-19: Added tracking domain models (TrackingPageAggregate, VisitEventEntity, PageSettings), new EF Core persistence entities/configuration, and migration `20260419101500_AddTrackingPersistence` with explicit `Down` rollback for CI/local forward-backward migration verification.

## Phase 2 - Application
- [x] Implement commands for tracking page create/update/publish/archive/delete flows.
- [x] Implement commands for visit ingestion and deduplication guard behavior.
- [x] Implement queries for page detail/list and analytics read models.
- [x] Add FluentValidation validators for all tracking commands/queries.
- [x] Define DTOs/contracts for tracking page CRUD and analytics outputs.
- [x] Configure mapping profiles between Domain, persistence models, and DTOs.
- [x] Ensure handlers are cancellation-aware and use async I/O patterns.


Phase 2 evidence:
- 2026-04-19: Added Tracking CQRS command/query flows with validation, DTO contracts, mapping profiles, EF Core repositories, and visit deduplication/analytics read models for tracking pages and visit events.

## Phase 3 - API
- [x] Implement public landing endpoint `GET /p/{slug}` for page resolution and visitor flow entry.
- [x] Implement visit capture endpoint for client/server visit logging (e.g., POST visit endpoint).
- [x] Implement authenticated admin CRUD endpoints for tracking pages.
- [x] Implement analytics endpoints for summary, trends, and recent events.
- [x] Enforce ProblemDetails-based error responses and request validation integration.
- [x] Ensure Swagger/OpenAPI documents all tracking and analytics contracts.


Phase 3 evidence:
- 2026-04-19: Added public tracking slug resolution endpoint (`GET /p/{slug}`), visit ingestion endpoint (`POST /api/tracking/pages/{trackingPageId}/visits`), authenticated tracking CRUD + analytics endpoints, and expanded ProblemDetails exception mapping for not found/conflict semantics used by tracking workflows.

## Phase 4 - Frontend
- [ ] Build admin dashboard shell for tracking overview.
- [ ] Build tracking pages grid (search, sort, status, pagination).
- [ ] Build tracking page editor (slug, metadata, settings, publish controls).
- [ ] Build analytics detail screen per tracking page.
- [ ] Integrate charts for trends and visit distributions.
- [ ] Add filters (date range, page, source/referrer, device/UA buckets).
- [ ] Ensure frontend consumes generated API proxies only (no handwritten duplicates).

## Phase 5 - Analytics
- [ ] Implement total visits metric pipeline.
- [ ] Implement unique visits metric strategy (session/fingerprint policy).
- [ ] Implement top pages ranking logic.
- [ ] Implement recent visits stream/query.
- [ ] Implement trend aggregation (hour/day/week windows).
- [ ] Define metric definitions and edge-case behavior (bot traffic, duplicate hits, timezone boundaries).

## Phase 6 - Security
- [ ] Enforce authentication for all admin and analytics management endpoints.
- [ ] Apply role-based authorization rules for Admin, Operator, Viewer.
- [ ] Add rate limiting for public tracking and visit ingestion endpoints.
- [ ] Harden request validation and payload size constraints.
- [ ] Ensure sensitive visitor fields follow privacy policy (hashing/masking/retention controls).
- [ ] Add security-focused logging/auditing for access and anomaly events.

## Phase 7 - DevOps
- [ ] Provide docker-compose setup for API, Gateway, database, and optional Redis.
- [ ] Normalize environment configuration templates for local/staging/production.
- [ ] Add structured logging sinks and correlation-id propagation.
- [ ] Add CI pipeline for build, test, lint, migration checks, and proxy sync checks.
- [ ] Add CD/release workflow baseline with environment-specific safeguards.

## Phase 8 - Testing
- [ ] Add unit tests for domain invariants and analytics calculation rules.
- [ ] Add unit tests for application handlers, validators, and mapping profiles.
- [ ] Add integration tests for tracking page CRUD and visit ingestion endpoints.
- [ ] Add integration tests for analytics endpoints with representative datasets.
- [ ] Add frontend UI smoke tests for dashboard, pages grid, page editor, and analytics detail flows.
- [ ] Add regression checks ensuring no legacy phishing/email behavior remains.

## Phase 9 - Polish
- [ ] Optimize hot paths (visit ingestion throughput, analytics query performance, indexing).
- [ ] Perform UX cleanup for layout consistency, readability, and responsive behavior.
- [ ] Finalize docs (architecture, API contracts, analytics definitions, runbooks).
- [ ] Run final production-readiness review across security, observability, and reliability.
- [ ] Confirm Definition of Done criteria are fully met before release.

## Definition of Done (Cross-Phase Exit Criteria)
- [ ] Builds pass for backend, gateway, and frontend.
- [ ] Runtime behavior verified for tracking creation, public page access, visit capture, and analytics display.
- [ ] Swagger/OpenAPI reflects all current contracts.
- [ ] Generated proxies are synchronized with backend contracts.
- [ ] No placeholder/fake data or deprecated phishing/email flows remain.
- [ ] Security, role authorization, and rate-limiting behavior are verified.
- [ ] TASKS.md statuses are accurate and evidence is documented.
