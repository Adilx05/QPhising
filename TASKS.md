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
- [x] Build admin dashboard shell for tracking overview.
- [x] Build tracking pages grid (search, sort, status, pagination).
- [x] Build tracking page editor (slug, metadata, settings, publish controls).
- [x] Build analytics detail screen per tracking page.
- [x] Integrate charts for trends and visit distributions.
- [x] Add filters (date range, page, source/referrer, device/UA buckets).
- [x] Ensure frontend consumes generated API proxies only (no handwritten duplicates).

Phase 4 evidence:
- 2026-04-19: Added authenticated `/tracking` frontend module with generated TrackingService-based data-access, admin KPI shell, searchable/sortable/paginated tracking page grid, editor + lifecycle controls, analytics detail with date/source/device filters, and trend/distribution chart visualizations.

## Phase 5 - Analytics
- [x] Implement total visits metric pipeline.
- [x] Implement unique visits metric strategy (session/fingerprint policy).
- [x] Implement top pages ranking logic.
- [x] Implement recent visits stream/query.
- [x] Implement trend aggregation (hour/day/week windows).
- [x] Define metric definitions and edge-case behavior (bot traffic, duplicate hits, timezone boundaries).


Phase 5 evidence:
- 2026-04-19: Added analytics overview CQRS + API endpoint (`GET /api/tracking/analytics/overview`) with total/unique visit pipelines, top-pages ranking, cross-page recent-visit stream, hour/day/week trend aggregation with timezone offsets, bot-exclusion support, and explicit metric definition semantics for duplicate/bot/time-boundary edge cases.
- 2026-04-19: Fixed analytics compile-time regressions by normalizing unique-visitor selector translation in EF Core repository and removing index-based access on IReadOnlyCollection in overview query handler.

## Phase 6 - Security
- [x] Enforce authentication for all admin and analytics management endpoints.
- [x] Apply role-based authorization rules for Admin, Operator, Viewer.
- [x] Add rate limiting for public tracking and visit ingestion endpoints.
- [x] Harden request validation and payload size constraints.
- [x] Ensure sensitive visitor fields follow privacy policy (hashing/masking/retention controls).
- [x] Add security-focused logging/auditing for access and anomaly events.

Phase 6 evidence:
- 2026-04-19: Finalized policy-based authentication/authorization coverage for management + analytics endpoints, added fixed-window rate limiting policies on public tracking routes, enforced visit payload constraints (timestamp validity + request body size), moved IP hash derivation to server-side privacy service with optional pepper, and added audit middleware logging for 401/403/429 security events.

## Phase 7 - DevOps
- [x] Provide docker-compose setup for API, Gateway, database, and optional Redis.
- [x] Normalize environment configuration templates for local/staging/production.
- [x] Add structured logging sinks and correlation-id propagation.
- [x] Add CI pipeline for build, test, lint, migration checks, and proxy sync checks.
- [x] Add CD/release workflow baseline with environment-specific safeguards.

Phase 7 evidence:
- 2026-04-19: Added root `docker-compose.yml` with API/Gateway/PostgreSQL and optional Redis profile, introduced environment templates under `deploy/env`, enabled JSON console logging plus `X-Correlation-Id` middleware in API/Gateway, and added `.github/workflows/ci.yml` + `.github/workflows/release.yml` for CI validation and environment-guarded release baseline.

## Phase 8 - Testing
- [x] Add unit tests for domain invariants and analytics calculation rules.
- [x] Add unit tests for application handlers, validators, and mapping profiles.
- [x] Add integration tests for tracking page CRUD and visit ingestion endpoints.
- [x] Add integration tests for analytics endpoints with representative datasets.
- [x] Add frontend UI smoke tests for dashboard, pages grid, page editor, and analytics detail flows.
- [x] Add regression checks ensuring no legacy phishing/email behavior remains.

Phase 8 evidence:
- 2026-04-19: Added comprehensive tracking test coverage in `backend/API.Tests` (domain invariants, application handler/validator/mapping behavior, role-aware tracking CRUD/visit ingestion/analytics integration flows, and legacy-route regression checks), introduced frontend UI smoke checker script (`scripts/check-frontend-ui-smoke.*`) for dashboard + tracking grid/editor/analytics presence, and wired smoke checks into CI.
- 2026-04-19: Expanded backend configuration test suite with runtime/setup validator and handler unit tests plus role/validation integration coverage for `/api/configuration`; added `backend/API.Tests/test.runsettings` and CI/release coverage collection flags for consistent backend test execution settings.
- 2026-04-23: Added `HealthReadinessUnitTests` coverage for operational health status mapping, health response serialization/sorting, Redis optional readiness degradation behavior, and Keycloak readiness success/failure guard paths.

## Phase 9 - Polish
- [x] Optimize hot paths (visit ingestion throughput, analytics query performance, indexing).
- [x] Perform UX cleanup for layout consistency, readability, and responsive behavior.
- [x] Finalize docs (architecture, API contracts, analytics definitions, runbooks).
- [x] Run final production-readiness review across security, observability, and reliability.
- [ ] Confirm Definition of Done criteria are fully met before release.

Phase 9 evidence:
- 2026-04-19: Reworked template management into HTML page-template flow (backend + frontend), added live preview support in templates UI, and introduced optional tracking page -> template linkage for page creation/update and public landing resolution contracts.
- 2026-04-19: Reduced analytics query latency by parallelizing page-analytics read tasks and moving top-pages aggregation work to database grouping in `EfCoreVisitEventRepository`; refreshed tracking dashboard UX copy/layout controls for responsive readability; and added final architecture/API/runbook/readiness docs under `docs/architecture` and `docs/operations`.
- 2026-04-19: Verified frontend build + tracking UI smoke checks locally; backend/gateway build verification remains blocked in this execution environment because `dotnet` CLI is unavailable.
- 2026-04-19: Rewrote root `README.md` in English to reflect the current production state (tracking/campaign lifecycle, analytics capabilities, security model, contract-first proxy workflow, and run/deployment quality gates).

## Definition of Done (Cross-Phase Exit Criteria)
- [ ] Builds pass for backend, gateway, and frontend.
- [ ] Runtime behavior verified for tracking creation, public page access, visit capture, and analytics display.
- [ ] Swagger/OpenAPI reflects all current contracts.
- [ ] Generated proxies are synchronized with backend contracts.
- [ ] No placeholder/fake data or deprecated phishing/email flows remain.
- [ ] Security, role authorization, and rate-limiting behavior are verified.
- [ ] TASKS.md statuses are accurate and evidence is documented.

## Incremental Feature Requests
- [x] Add queryable audit-log read model and admin/operator audit console with critical event taxonomy coverage.
- [x] Add configurable visitor IP capture policy (off/plain/hashed) and ensure visit ingestion records IP according to tracking-page settings.
- [x] Rework campaign creation flow to create a real tracking page (template-based or blank) without manual GUID entry in UI.
- [x] Add campaign detail route and screen reachable from campaign listing cards.
- [x] Persist campaign-to-tracking-page relationship in Domain + EF Core schema.
- [x] Fix campaign public landing availability and surface campaign-level click/unique-click analytics with configurable tracking-page privacy settings at create time.
- [x] Fix tracking analytics detail unique-visitor query runtime failure and ensure campaign detail shows persisted start/end schedule windows.
- [x] Introduce auditable BaseEntity + soft-delete semantics (`IsDeleted`) for core write-side entities and enforce global EF filtering with no hard deletes.
- [x] Add campaign delete action in campaigns list and cascade soft-delete linked tracking pages when campaign is deleted.
- [x] Remove Redis-only setup/runtime form requirements, add appsettings bootstrap guard fallback, and expose authenticated user card with sidebar logout action.
- [x] Hide Setup Wizard navigation once setup is completed and move Runtime Configuration into a low-prominence bottom sidebar section while placing the user card at the top.
- [x] Replace setup-state home dashboard with application analytics dashboard (campaign totals, tracking summary, trends, and recent visits) using PrimeNG data components and live backend metrics.
- [x] Simplify sidebar user card content to show only full name and role, and normalize dashboard copy/date formatting for proper Turkish character rendering.
- [x] Ensure campaign-created public pages stay inaccessible until campaign lifecycle is Active, and return 404 for Draft/Paused/non-active lifecycle states.
- [x] Add app-shell language selector and dark-mode toggle in top bar, ensure Turkish campaign naming uses "Senaryo", and hide sidebar user card for unauthenticated sessions.
- [x] Expand language support to full-page TR/EN localization across auth, dashboard, setup/runtime configuration, template management, and tracking screens.
- [x] Add analytics report export center with CSV/PDF outputs (global/selected scope, summary/detailed levels, selectable/all-time ranges, and TR/EN localization).
- [x] Improve report center selected-tracking-page UX and enrich PDF export layout with chart/table sections plus visitor IP/session click breakdown toggle.
- [x] Remove setup wizard and runtime configuration surfaces from frontend/backend, rely on appsettings-based Keycloak/PostgreSQL configuration, and always run EF Core migration checks on API startup.
- [x] Split API/Gateway health model into liveness/readiness endpoints with standardized dependency payload, dependency-specific readiness probes (PostgreSQL, Redis optional degrade, Keycloak timeout probe), and admin dashboard readiness summary card.
- [x] Move the System Health summary card from dashboard content into the sidebar bottom section with a compact read-only layout.
- [x] Finalize frontend container runtime configuration flow with multi-stage build, Nginx SPA fallback, and env-driven `runtime-config.js` generation at startup.
- [x] Remove frontend dark-mode support and theme-toggle behavior across shell/preferences/styles so application remains light-only.
- [x] Create a professional GitHub Pages static product website under `docs/` with light enterprise SaaS styling, architecture/feature sections, and deployment-ready assets.

Incremental evidence:
- 2026-04-25: Improved `scripts/validate-proxy-generation.sh` proxy drift failure diagnostics by emitting `git status`, `git diff --numstat`, and a capped diff preview for `frontend/src/app/shared/proxy`, making CI proxy-sync failures directly actionable.
- 2026-04-25: Removed JSON comments from `backend/Gateway/ocelot.json` so gateway route configuration remains strict-JSON compliant for CI proxy/gateway consistency validation (`check-proxy-gateway-consistency.sh`).
- 2026-04-25: Updated `scripts/check-frontend-ui-smoke.js` to validate authentication guards (`authenticationCanActivateGuard`/`authenticationCanMatchGuard`) on protected frontend routes after setup wizard/guard removal, preventing stale setup-guard assertion failures in UI smoke checks.
- 2026-04-25: Removed frontend dark-mode infrastructure end-to-end by deleting theme state/toggle methods from user preferences and app shell, removing PrimeNG dark-mode selector wiring, and deleting `body.app-dark` style overrides so UI runs in light mode only.
- 2026-04-25: Added a GitHub Pages-ready static marketing site in `docs/` (`index.html`, `styles.css`, `script.js`, and SVG assets) with light enterprise design, responsive navigation, product architecture/features/install sections, and documentation links mapped to current repository artifacts.
- 2026-04-25: Cleaned up redundant standalone Swagger/proxy GitHub Actions workflows (checks remain covered by `ci.yml`) and added automated GitHub Pages deployment workflow (`.github/workflows/github-pages.yml`) that publishes the `docs/` directory on `main` pushes and manual dispatch.
- 2026-04-25: Fixed Docker Compose API startup health gating by making Keycloak readiness probe configurable (`HealthChecks:Keycloak:Enabled`) and setting Compose default to disabled, so `/health/ready` stays non-unhealthy when Keycloak is intentionally not part of local container stack; added unit coverage for disabled-probe degraded status.
- 2026-04-24: Finalized frontend Docker runtime config flow by keeping a Node build + Nginx serve multi-stage Dockerfile, generating `/usr/share/nginx/html/runtime-config.js` from container env via `frontend/docker/entrypoint.sh`, loading runtime config in `index.html` before Angular bundles, and enforcing SPA fallback with `try_files $uri /index.html` in Nginx.
- 2026-04-24: Kept gateway downstream health defaults in `backend/Gateway/appsettings.json` as localhost for local development, added Docker Compose gateway environment overrides for `HealthChecks__DownstreamApi__Host/Port/Scheme` (defaulting to `api:5050` over HTTP), and documented service-name-based downstream host requirement for container networks in `README.md`.
- 2026-04-24: Updated root Docker Compose topology by removing built-in PostgreSQL from base stack, wiring API connection string from environment, adding API/Gateway readiness healthchecks with health-based dependency ordering, and adding a frontend container with runtime-injected gateway/auth configuration.
- 2026-04-23: Fixed `TrackingEndpointsIntegrationTests` create-flow collisions by generating per-test unique campaign/tracking slugs and names, preventing cross-test data conflicts (`409 Conflict`) when integration fixtures reuse persisted state.
- 2026-04-23: Hardened campaign integration-test setup with conflict-aware retry helper (`CreateCampaignWithUniqueSlugAsync`) so campaign-create tests self-heal from rare persisted slug collisions and keep lifecycle/delete assertions deterministic.
- 2026-04-23: Expanded audit taxonomy coverage to include template save/delete, campaign lifecycle transitions (start/pause/complete/cancel), and tracking publish/archive/delete actions; also updated tracking delete command flow to automatically cancel any linked non-terminal campaign before tracking-page soft-delete so orphaned active campaigns cannot remain.
- 2026-04-23: Added queryable `audit_log_entries` read model (timestamp/actor/action/resource/outcome/outcomeCode/correlationId/ipHash), CQRS audit query use-case (date/user/endpoint/result filters + pagination/sorting), operator/admin `GET /api/audit/logs` endpoint with FluentValidation/ProblemDetails integration, frontend `/audit-logs` table + quick filters + correlation search + event detail drawer, and documented critical event taxonomy (`401/403/429`, `campaign.delete`, `template.publish`) in `docs/operations/audit-event-taxonomy.md`.
- 2026-04-19: Fixed `RuntimeConfigurationResult` unit-test regressions in `backend/API.Tests/BackendConfigurationUnitTests.cs` by asserting configuration flags from the result contract and validating persisted Keycloak tuple values from repository state.
- 2026-04-19: Removed Redis input/test requirements from setup + runtime configuration flows (Redis now optional in setup/runtime aggregates), setup guard now allows main app when base appsettings bootstrap config is already present, and app shell now renders authenticated user card (name/role) with sidebar logout action.
- 2026-04-19: Campaign create flow now provisions public landing validity window + optional custom HTML, removes destination URL dependency, returns/create-UI displays slug/id public links, and public `/p/{slug}` resolution now accepts optional `id`/`campaign` query with strict 404 for unpublished or out-of-window pages.
- 2026-04-19: Campaign create contract now accepts tracking page fields and optional template; handler provisions tracking page + campaign atomically via CQRS, campaign aggregate now persists `TrackingPageId`, and migration `20260419140000_CampaignTrackingPageLink` introduces campaign→tracking_page FK/index.
- 2026-04-19: Campaign UI now uses template dropdown (no GUID input), captures page slug/title/destination for create flow, lists campaigns with detail navigation, and adds dedicated `/campaigns/:campaignId` detail page.
- 2026-04-19: Fixed campaign start lifecycle to allow Draft→Active transition, restored campaign HTML preview reactivity (custom HTML + selected template fallback), enriched campaign detail page with public links and live preview, and isolated `/p/:slug` routes from admin shell so public landing pages render standalone with strict 404 fallback.
- 2026-04-19: Fixed tracking update command wiring in API controller, enforced campaign-linked tracking-page auto-publish on campaign create/start to prevent false 404 on valid windows, applied ingest-time bot filtering/IP masking/UTM stripping policies from page settings, normalized unique-visitor metrics to session/fingerprint/IP fallback keys, and surfaced total/unique click metrics on campaign detail with create-flow controls for retention/privacy/bot/UTM settings.
- 2026-04-19: Fixed per-page analytics 409 failure by making EF-translatable unique-visitor key selection in `EfCoreVisitEventRepository`, and updated campaign detail UI to display schedule from campaign window with tracking validity fallback so persisted start/end dates are visible.
- 2026-04-19: Fixed remaining analytics 409/runtime faults by removing parallel query execution against a shared EF Core DbContext in tracking analytics handlers; wired public `/p/:slug` page load to call visit-ingestion API with generated session/fingerprint identifiers; and made campaign detail public links visible/clickable as absolute URLs.
- 2026-04-19: Added anonymous slug-based visit ingestion route (`POST /api/tracking/pages/{slug}/visits`) in API + Gateway so slug-entry traffic can record visits without JWT, while still reusing public landing-page accessibility checks.
- 2026-04-19: Stabilized unique-visitor analytics counting by replacing string-interpolated distinct keys with EF-translatable typed key projection (`KeyType` + `KeyValue`) in `EfCoreVisitEventRepository`, resolving `InvalidOperationException` during unique visitor count queries.
- 2026-04-19: Fixed duplicate public slug click ingestion by adding short-lived sessionStorage throttling in `PublicTrackingLandingPageComponent`, preventing accidental double visit POSTs caused by rapid duplicate page-initialization events.
- 2026-04-19: Fixed duplicate public landing slug GET/click request issue caused by initial shell/public-layout router-outlet swap; `AppComponent` now resolves first-render URL from `window.location` so `/p/:slug` renders directly in public layout without double component initialization.
- 2026-04-19: Fixed dashboard analytics overview 409/runtime failure by replacing non-translatable `IsBotUserAgent` method usage in `EfCoreVisitEventRepository` query filters with EF-translatable PostgreSQL `ILIKE` predicates for bot signature checks.
- 2026-04-19: Stopped campaign-create flow from auto-publishing linked tracking pages, and enforced public `/p/{slug}` resolution to return 404 whenever a linked campaign lifecycle state is not `Active` (including Draft and Paused); added integration coverage for Draft→Active→Paused accessibility transitions.

- 2026-04-19: Added tracking-page IP capture controls (`CaptureIpAddress` + `IpAddressHashPolicy` plain/SHA-256), wired landing-page visit capture to apply page policy, and fixed server-side ingestion/storage mapping so IP data is persisted according to selected privacy mode.
- 2026-04-19: Added domain/infrastructure base entity abstractions for shared identity/audit fields, introduced mandatory `is_deleted` persistence for campaign/template/tracking-page records, switched delete handlers/repositories to soft delete only, and configured EF Core global query filters + filtered unique indexes to automatically exclude deleted rows.
- 2026-04-19: Extended soft-delete metadata with optional `deleted_at_utc` and `deleted_by` fields across domain + persistence base entities, propagated metadata through aggregate rehydration/mapping/repositories, and added migration `20260419193000_AddSoftDeleteMetadataColumns`.
- 2026-04-19: Added campaign list delete action (Admin-only UI control), wired campaign data-access `campaignDelete` proxy usage, and updated `DeleteCampaignCommandHandler` to soft-delete linked tracking pages automatically; validated with integration coverage for campaign+tracking page post-delete 404 behavior.
- 2026-04-19: Updated app shell navigation to auto-hide Setup Wizard after setup readiness is `Ready`, moved the authenticated user card to the top of the sidebar, and repositioned Runtime Configuration into a bottom, lower-prominence system section.
- 2026-04-19: Replaced `/dashboard` setup health view with a production-focused analytics dashboard powered by campaign list, tracking page list, and tracking overview endpoints; added PrimeNG table/tag/progress widgets for KPI, top-page, trend, and recent-visit summaries; simplified sidebar user card copy to name+role only; and standardized dashboard Turkish text/date formatting (`tr-TR`).

- 2026-04-19: Added app-shell language selector (TR/EN) + theme toggle controls on the top bar, localized campaign-area copy with Turkish "Senaryo" naming, and hid the sidebar authenticated-user card whenever no session is active.
- 2026-04-19: Expanded bilingual UI coverage to all major frontend pages (auth callback/unauthorized, dashboard, setup wizard, runtime configuration, templates, tracking dashboard, and public landing fallback) with consistent TR/EN copy driven by language preference.
- 2026-04-20: Fixed JWT claim decoding to properly parse UTF-8 payloads in frontend OIDC auth service so Turkish characters (e.g., `KÖSE`) render correctly in the authenticated user card instead of mojibake.
- 2026-04-20: Added authenticated `/reports` export center with TR/EN localized controls for scope/detail/range presets, implemented backend CQRS export endpoint (`GET /api/tracking/analytics/reports/export`) for CSV/PDF generation (global or selected tracking page), and extended analytics repository/reporting services with visitor click aggregation for detailed exports.
- 2026-04-20: Reports UI now refreshes/guards selected tracking-page scope with explicit tracking-page chooser state, added optional visitor/IP click list toggle for exports, and upgraded PDF export content blocks to include KPI + trend bar chart + distribution + visitor detail table with improved Turkish character normalization.
- 2026-04-20: Fixed Reports scope-selection regression where "Selected Tracking Page" did not reveal page chooser (non-reactive computed over plain field); updated Reports page to evaluate scope requirement directly and split inline template into dedicated HTML + TS files for maintainability.
- 2026-04-20: Fixed reports export download failure (`Failed to execute 'createObjectURL'`) by normalizing proxy export payloads into `Blob` instances before triggering browser download.
- 2026-04-20: Fixed PDF download corruption/blank-page issue by deferring object-URL revocation after browser download trigger and removing leftover debug disk-write side effect in backend PDF exporter.
- 2026-04-20: Reworked report export contract to return `TrackingReportFileResult` JSON payload (base64 content + filename/contentType) directly from controller so generated proxies remain stable after regeneration; frontend export flow now decodes base64 safely into Blob and uses server-provided filename when available.
- 2026-04-20: Fixed report trend-chart KPI mismatch by switching report trend rows to cumulative total-click rendering (final point now aligns with summary total clicks) and aligned tracking-page report trend query defaults to true all-time (`UnixEpoch` -> `UtcNow`) when no explicit date range is provided.
- 2026-04-23: Removed setup/runtime configuration controllers, CQRS flows, frontend pages/guards/navigation, and gateway setup route forwarding; API/Gateway startup now reads static appsettings configuration (no runtime override file) and API always executes EF Core migration check (`Database.Migrate`) during startup.

- 2026-04-23: Updated proxy-generation and Swagger quality-gate scripts to remove removed setup/runtime-configuration contract prerequisites (`/api/configuration`, `/api/setup/*`) so contract validation now targets active endpoints only.

- 2026-04-23: Updated API and Gateway health architecture to expose `/health/live` and `/health/ready` with a shared response contract (overall status, dependency list, latency, failure reason); added real PostgreSQL connectivity check, optional Redis degrade probe, timeout-based Keycloak metadata/token readiness checks, gateway downstream API readiness probe, refreshed operations runbooks, and surfaced read-only system health summary in admin dashboard.
- 2026-04-23: Moved frontend System Health summary from dashboard widget area into the left sidebar bottom card, preserving readiness/dependency/latency data and bilingual labels while keeping dashboard focused on analytics content.
