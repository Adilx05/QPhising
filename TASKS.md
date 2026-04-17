# TASKS

Status legend:
- [ ] Pending
- [-] In Progress
- [x] Completed

Metadata required for every subtask:
- StartedAt:
- FinishedAt:
- Owner:
- Notes:

## Phase 1 - Repository Reset & Bootstrap

- [x] 1.1 Backend domain/model baseline (solution skeleton, bounded contexts, domain folders)
  - StartedAt: 2026-04-17T08:10:00Z
  - FinishedAt: 2026-04-17T08:25:00Z
  - Owner: Codex
  - Notes: Repository reset and clean architecture folder topology established.
- [x] 1.2 Application logic baseline (CQRS project scaffolding + contracts folders)
  - StartedAt: 2026-04-17T08:25:00Z
  - FinishedAt: 2026-04-17T08:35:00Z
  - Owner: Codex
  - Notes: Application layer scaffolded for backend-first flow.
- [x] 1.3 Endpoint baseline (API host skeleton and controller structure)
  - StartedAt: 2026-04-17T08:35:00Z
  - FinishedAt: 2026-04-17T08:45:00Z
  - Owner: Codex
  - Notes: HTTP entrypoint structure created with transport-only intent.
- [x] 1.4 Swagger baseline (OpenAPI bootstrapping verified)
  - StartedAt: 2026-04-17T08:45:00Z
  - FinishedAt: 2026-04-17T08:50:00Z
  - Owner: Codex
  - Notes: Swagger foundation included in bootstrap baseline.
- [x] 1.5 Proxy generation baseline (scripts/docs placeholders wired)
  - StartedAt: 2026-04-17T08:50:00Z
  - FinishedAt: 2026-04-17T08:55:00Z
  - Owner: Codex
  - Notes: Proxy generation path established for frontend dependency.
- [x] 1.6 Frontend integration baseline (feature shell folders only)
  - StartedAt: 2026-04-17T08:55:00Z
  - FinishedAt: 2026-04-17T09:00:00Z
  - Owner: Codex
  - Notes: Frontend scaffolding created without bypassing backend-first constraints.

## Phase 2 - Proxy Generation System Hardening

- [x] 2.1 Backend domain/model contract-source boundaries defined per module
  - StartedAt: 2026-04-17T09:00:00Z
  - FinishedAt: 2026-04-17T09:10:00Z
  - Owner: Codex
  - Notes: Contract ownership clarified to prevent proxy drift.
- [-] 2.2 Application logic for contract-drift validation workflow
  - StartedAt: 2026-04-17T09:10:00Z
  - FinishedAt:
  - Owner: Codex
  - Notes: Implement stale-proxy detection workflow and failure semantics.
- [ ] 2.3 Endpoint for proxy validation/check command invocation
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 2.4 Swagger validation gate integration for proxy generation preconditions
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 2.5 Proxy generation script finalization (cross-platform + deterministic outputs)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 2.6 Frontend integration verification using generated clients only
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 3 - Setup Wizard Backend APIs

- [ ] 3.1 Backend domain/model for setup aggregate, readiness state, and secure config value objects
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 3.2 Application logic (commands/queries/handlers/validators for status, test, save)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 3.3 Endpoint implementation (`/api/setup/status`, `/test-db`, `/test-keycloak`, `/test-redis`, `/save`)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 3.4 Swagger contract verification for setup endpoints and examples
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 3.5 Proxy generation for setup controller and DTOs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 3.6 Frontend integration for setup flow using generated setup proxy
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 4 - Setup Wizard Gating & UX Completion

- [ ] 4.1 Backend domain/model for setup completion policy and access state
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 4.2 Application logic for setup-complete checks, redirect decisions, and guard contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 4.3 Endpoint for setup completion/readiness and guarded app-access checks
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 4.4 Swagger verification for gating/readiness contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 4.5 Proxy generation refresh for setup gating contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 4.6 Frontend integration (route guards + redirect to setup until complete)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 5 - Runtime Configuration Persistence

- [ ] 5.1 Backend domain/model for runtime configuration aggregate and secret-handling rules
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 5.2 Application logic for read/write/update configuration use cases + validators
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 5.3 Endpoint implementation for configuration operations and readiness checks
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 5.4 Swagger verification for runtime configuration contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 5.5 Proxy generation for runtime configuration APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 5.6 Frontend integration for secure runtime configuration management screens
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 6 - Backend Foundation

- [ ] 6.1 Backend domain/model shared primitives (entity/value object/domain event base)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 6.2 Application logic cross-cutting pipeline behaviors (validation/logging/authz)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 6.3 Endpoint pipeline integration (ProblemDetails, middleware, DI wiring)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 6.4 Swagger standard behavior checks for global error/auth response documentation
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 6.5 Proxy generation compatibility validation against base API conventions
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 6.6 Frontend integration baseline for common API error/auth handling
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 7 - Swagger Standards

- [ ] 7.1 Backend domain/model conventions for API resource naming and version scope
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 7.2 Application logic conventions for operation IDs, response envelopes, and examples
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 7.3 Endpoint annotation updates to enforce standard OpenAPI metadata
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 7.4 Swagger generation quality gate in CI/local checks
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 7.5 Proxy generation validation against standardized Swagger docs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 7.6 Frontend integration sanity check across standardized generated proxies
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 8 - Gateway (Ocelot)

- [ ] 8.1 Backend domain/model for gateway route ownership and module map definitions
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 8.2 Application logic for route policy composition (auth forwarding, claims mapping)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 8.3 Endpoint/gateway route implementation and middleware behavior
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 8.4 Swagger alignment check for gateway-exposed downstream services
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 8.5 Proxy generation consistency check with gateway routing strategy
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 8.6 Frontend integration smoke validation through gateway paths
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 9 - Identity (Keycloak)

- [ ] 9.1 Backend domain/model for role model (`Admin`, `Operator`, `Viewer`) and claims mapping
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 9.2 Application logic for authorization policies and token validation flows
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 9.3 Endpoint protection and policy binding across protected resources
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 9.4 Swagger security scheme and protected endpoint authorization metadata verification
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 9.5 Proxy generation refresh for secured endpoints and auth headers
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 9.6 Frontend integration for authenticated flows and role-based UX access
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 10 - Database & Migrations

- [ ] 10.1 Backend domain/model persistence boundaries and aggregate mappings
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 10.2 Application logic for transactional consistency, repositories, and unit-of-work rules
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 10.3 Endpoint behavior validation against persisted workflows
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 10.4 Swagger contract checks for persistence-backed request/response models
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 10.5 Proxy generation refresh for migration-aligned contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 10.6 Frontend integration verification with database-backed live flows
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 11 - Campaign Module

- [ ] 11.1 Backend domain/model for campaign entities, value objects, and lifecycle rules
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 11.2 Application logic for campaign CQRS handlers and validators
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 11.3 Endpoint implementation for campaign CRUD/lifecycle actions
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 11.4 Swagger verification for campaign contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 11.5 Proxy generation for campaign APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 11.6 Frontend integration for campaign features using generated proxies
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 12 - Template Module

- [ ] 12.1 Backend domain/model for template content, metadata, and versioning constraints
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 12.2 Application logic for template commands/queries and validation
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 12.3 Endpoint implementation for template operations
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 12.4 Swagger verification for template endpoint contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 12.5 Proxy generation for template APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 12.6 Frontend integration for template management via generated proxies
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 13 - Tracking Module

- [ ] 13.1 Backend domain/model for tracking events, correlations, and transitions
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 13.2 Application logic for tracking ingestion/query handlers and validators
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 13.3 Endpoint implementation for tracking ingest and retrieval
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 13.4 Swagger verification for tracking contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 13.5 Proxy generation for tracking APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 13.6 Frontend integration for tracking views via generated proxies
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 14 - Background Jobs

- [ ] 14.1 Backend domain/model for job types, retry/idempotency policies
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 14.2 Application logic for job dispatch/execution and guardrails
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 14.3 Endpoint implementation for operational job status/control
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 14.4 Swagger verification for background job operational contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 14.5 Proxy generation for job operations endpoints
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 14.6 Frontend integration for job monitoring and controls
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 15 - Analytics

- [ ] 15.1 Backend domain/model for analytics metrics and aggregation boundaries
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 15.2 Application logic for analytics queries/services and validation rules
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 15.3 Endpoint implementation for analytics data retrieval
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 15.4 Swagger verification for analytics contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 15.5 Proxy generation for analytics APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 15.6 Frontend integration for analytics dashboards with real API data
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 16 - Export System

- [ ] 16.1 Backend domain/model for export request lifecycle and audit fields
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 16.2 Application logic for export commands/queries and validation
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 16.3 Endpoint implementation for export request/status/download
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 16.4 Swagger verification for export contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 16.5 Proxy generation for export APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 16.6 Frontend integration for export flows using generated proxies
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 17 - Frontend Platform (Generated Proxies Only)

- [ ] 17.1 Backend domain/model confirmation for all frontend-required contracts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 17.2 Application logic readiness validation for all consumed use cases
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 17.3 Endpoint readiness verification before UI implementation
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 17.4 Swagger completeness validation for all frontend feature APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 17.5 Full proxy regeneration + drift check and commit of generated artifacts
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 17.6 Frontend integration completion (guards, forms, role access, real API UX states)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 18 - Build / Publish Scripts

- [ ] 18.1 Backend domain/model for release artifact boundaries and package manifest rules
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 18.2 Application logic for build/test/publish orchestration workflow
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 18.3 Endpoint/tool entrypoints for script execution and environment validation
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 18.4 Swagger verification for any build/publish operational APIs
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 18.5 Proxy generation update for operational endpoints (if exposed)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 18.6 Frontend integration for release/operator tooling surfaces (if applicable)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Phase 19 - Final Validation & Production Readiness

- [ ] 19.1 Backend domain/model audit (no placeholder/fake model artifacts remain)
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 19.2 Application logic audit + full test execution evidence
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 19.3 Endpoint/runtime smoke validation and security behavior verification
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 19.4 Swagger final verification across all modules
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 19.5 Final proxy regeneration + zero-drift confirmation
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:
- [ ] 19.6 Frontend integration final compile/sanity validation with real APIs only
  - StartedAt:
  - FinishedAt:
  - Owner:
  - Notes:

## Definition of Done (Cross-Phase Exit Criteria)

- [ ] Backend build passes (attach command + timestamp + result evidence).
- [ ] Runtime behavior verified (record scenarios and observed outcomes).
- [ ] Swagger reflects contract changes (capture OpenAPI verification evidence).
- [ ] Proxies regenerated and synced (record generation command and drift result).
- [ ] Frontend compile passes (attach build output evidence).
- [ ] No placeholder/fake data remains (document cleanup audit).
- [ ] `TASKS.md` updated with evidence for completed tasks.
- [ ] Security and role rules verified for protected endpoints.
