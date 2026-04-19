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
- [ ] Audit repository for legacy phishing/email/mail concepts across Domain, Application, Infrastructure, API, Gateway, and Frontend.
- [ ] Remove or rename legacy phishing/email entities, value objects, enums, services, controllers, DTOs, and UI labels.
- [ ] Remove obsolete routes/endpoints and deprecated proxy surfaces tied to phishing/email workflows.
- [ ] Inspect package and project dependencies; remove unused libraries and stale references.
- [ ] Update naming conventions and module boundaries to align with Web Page Tracking & Visitor Analytics terminology.

## Phase 1 - Domain & Database
- [ ] Define `TrackingPage` aggregate (identity, slug, metadata, publish state, ownership).
- [ ] Define `VisitEvent` aggregate/entity (timestamp, page reference, visitor fingerprint/session, user-agent, referrer, IP-hash policy).
- [ ] Define optional `PageSettings` model (retention, privacy options, bot filtering, UTM capture toggles).
- [ ] Add domain invariants and value objects for slugs, URLs, and tracking identifiers.
- [ ] Implement EF Core entity configurations for tracking models.
- [ ] Add database migration(s) for tracking pages, visit events, and supporting indexes.
- [ ] Validate migration rollback/forward strategy for local and CI environments.

## Phase 2 - Application
- [ ] Implement commands for tracking page create/update/publish/archive/delete flows.
- [ ] Implement commands for visit ingestion and deduplication guard behavior.
- [ ] Implement queries for page detail/list and analytics read models.
- [ ] Add FluentValidation validators for all tracking commands/queries.
- [ ] Define DTOs/contracts for tracking page CRUD and analytics outputs.
- [ ] Configure mapping profiles between Domain, persistence models, and DTOs.
- [ ] Ensure handlers are cancellation-aware and use async I/O patterns.

## Phase 3 - API
- [ ] Implement public landing endpoint `GET /p/{slug}` for page resolution and visitor flow entry.
- [ ] Implement visit capture endpoint for client/server visit logging (e.g., POST visit endpoint).
- [ ] Implement authenticated admin CRUD endpoints for tracking pages.
- [ ] Implement analytics endpoints for summary, trends, and recent events.
- [ ] Enforce ProblemDetails-based error responses and request validation integration.
- [ ] Ensure Swagger/OpenAPI documents all tracking and analytics contracts.

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
