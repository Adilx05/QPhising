# OpenAPI Client Generation Strategy Decision Record

- Date: 2026-03-28
- Status: Approved
- Scope: Frontend Angular client generation for backend API contracts
- Related task: `18.2 / Evaluate and select OpenAPI generation strategy`

## Context

The frontend currently has no API transport wiring and is scheduled to integrate backend endpoints using generated OpenAPI clients. The backend API already exposes an OpenAPI document in development via ASP.NET OpenAPI middleware (`AddOpenApi` + `MapOpenApi`) and uses API versioning (`v1` group). Generated client output must align with the existing Angular feature architecture and auth interceptor flow.

## Decision Criteria

The selected generator must satisfy all of the following:

1. **Type safety** for request/response models and enum handling in TypeScript.
2. **Angular interceptor compatibility** (Keycloak bearer attachment, centralized error handling).
3. **CI reproducibility** with deterministic output and explicit version pinning.
4. **Maintainability** in a feature-based Angular codebase, keeping generated transport code separated from UI components.

## Options Considered

## Option A — NSwag (`nswag` / `NSwag.CodeGeneration.TypeScript`)

### Strengths
- Native .NET ecosystem alignment (same stack as backend).
- Good TypeScript model generation and API client scaffolding.
- Supports custom templates and operation grouping.

### Risks / Constraints
- Angular-focused output patterns are less idiomatic for modern standalone Angular compared to OpenAPI Generator’s `typescript-angular` templates.
- Additional customization effort expected for interceptor-first patterns and strict feature-oriented module boundaries.
- In prior team setups, long-term maintenance often drifts toward template patching.

## Option B — OpenAPI Generator (`typescript-angular`)

### Strengths
- Mature Angular-focused generator with explicit `typescript-angular` target.
- Strong compatibility with Angular `HttpClient`, DI services, and interceptor chains.
- Supports deterministic generation through pinned CLI version and config file.
- Better fit for separating generated data-access clients from feature facades.

### Risks / Constraints
- Requires Java runtime in generation environments.
- Generated output can be noisy without strict generator options and cleanup conventions.

## Decision

**Selected: OpenAPI Generator with the `typescript-angular` generator.**

### Rationale

OpenAPI Generator best satisfies the required criteria for Angular compatibility and maintainability while preserving strong type safety and reproducible CI generation. Its native Angular target reduces bespoke template work and keeps generated clients aligned with interceptor-based auth and feature-facade patterns.

## Version Pinning

Pin these versions for determinism:

- OpenAPI Generator CLI: `7.14.0`
- Generator target: `typescript-angular`
- Angular compatibility target for generated code: Angular `20.x` (current frontend baseline)

> Note: version pinning will be enforced in the upcoming generation-input subtask via committed config/command scripts.

## Integration Constraints

1. Generated files must live under a dedicated generated boundary (e.g. `frontend/src/app/core/api/generated/`) and be treated as non-hand-edited artifacts.
2. Feature modules/components must **not** call generated classes directly; they should consume feature data-access facades/adapters.
3. Auth, correlation, and error translation must remain centralized in Angular interceptors/wrappers, not duplicated in generated code.
4. Contract-to-UI mapping stays in handwritten mapper/facade layers to avoid leaking transport DTOs into presentation state.
5. Regeneration must be deterministic in CI and fail when generated artifacts drift from committed state.

## Consequences

### Positive
- Consistent typed API surface for all frontend features.
- Lower risk of handwritten HTTP drift.
- Easier onboarding and repeatable contract updates.

### Trade-offs
- Requires disciplined regeneration workflow and version lock maintenance.
- Adds generated artifact review overhead in pull requests.

## Follow-up Tasks

- Define authoritative OpenAPI source and deterministic generation command inputs.
- Establish generated-client module structure and ownership boundaries.
- Define regeneration + drift detection workflow (local + CI).
- Plan staged replacement of any handwritten duplicate API services.
