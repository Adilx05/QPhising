# Generated API Client Module Structure Blueprint

- Date: 2026-03-28
- Status: Approved
- Related task: `18.2 / Create typed generated client module structure`
- Depends on:
  - `docs/openapi-generation-strategy-decision.md`
  - `docs/openapi-source-of-truth-and-generation-inputs.md`

## 1) Goals and Constraints

This blueprint defines where generated OpenAPI artifacts live, which layers can import them, and how frontend features consume typed API contracts without leaking transport concerns into presentation components.

### Design goals
1. Keep generated code isolated and fully replaceable by regeneration.
2. Preserve Angular feature-based architecture boundaries.
3. Ensure all UI-facing code depends on facades/adapters, never direct `HttpClient` transport details.
4. Enable type-safe endpoint consumption using generated services and models.

### Non-goals
- No hand-editing under generated output.
- No direct generated-client injection in smart/dumb components.

## 2) Canonical Folder Structure

```text
frontend/src/app/core/api/
├── generated/                        # machine-generated only
│   ├── api/                          # OpenAPI Generator service classes
│   ├── model/                        # OpenAPI Generator DTO models
│   ├── configuration.ts
│   ├── variables.ts
│   └── index.ts
├── client/                           # handwritten transport composition for generated layer only
│   ├── api-client.providers.ts       # providers for base path/configuration
│   └── api-client.tokens.ts          # runtime config tokens (gateway base URL)
├── facades/                          # handwritten orchestration consumed by features
│   ├── campaigns-api.facade.ts
│   ├── templates-api.facade.ts
│   ├── tracking-api.facade.ts
│   ├── analytics-api.facade.ts
│   ├── tasks-api.facade.ts
│   └── exports-api.facade.ts
├── mappers/                          # DTO -> UI model mapping only
│   ├── campaigns.mapper.ts
│   ├── templates.mapper.ts
│   ├── tracking.mapper.ts
│   ├── analytics.mapper.ts
│   ├── tasks.mapper.ts
│   └── exports.mapper.ts
├── errors/                           # handwritten API error normalization
│   └── api-error.mapper.ts
└── index.ts                          # public exports for core/api consumers
```

## 3) Ownership and Edit Rules

| Path | Owner | Editable | Purpose |
|---|---|---|---|
| `core/api/generated/**` | OpenAPI generator | No (regeneration only) | Typed transport services and DTOs from backend OpenAPI. |
| `core/api/client/**` | Frontend platform layer | Yes | Runtime wiring (`Configuration`, base path, interceptors compatibility). |
| `core/api/facades/**` | Feature data-access layer | Yes | Feature-safe API methods returning UI-facing observables/models. |
| `core/api/mappers/**` | Feature/domain UI adapter layer | Yes | Convert generated DTOs to presentational models with null-safe defaults. |
| `core/api/errors/**` | Frontend platform layer | Yes | Map transport/problem-details payloads to typed app errors. |

## 4) Dependency Boundaries (Allowed Imports)

### Allowed
1. `features/**` -> `core/api/facades/**`
2. `core/api/facades/**` -> `core/api/generated/**`, `core/api/mappers/**`, `core/api/errors/**`
3. `core/api/generated/**` -> generator-internal files only
4. `core/api/client/**` -> `core/api/generated/configuration.ts` and Angular DI/runtime config

### Forbidden
1. `features/**` -> `core/api/generated/**` (prevents transport leakage)
2. `shared/components/**` -> any API client layer (dumb components stay pure)
3. `core/state/**` -> generated services directly (must use facade abstraction)

## 5) Feature-to-Client Mapping Strategy

The generated services remain contract-oriented, while facades provide feature-oriented methods:

- Campaigns feature facade wraps generated campaign endpoints and exposes list/detail/create/update/activate/schedule methods.
- Templates feature facade wraps template endpoints.
- Tracking feature facade wraps event/list endpoints.
- Analytics feature facade wraps KPI/trend endpoints.
- Tasks feature facade wraps queue/task endpoints.
- Exports feature facade wraps export request/status/download endpoints.

Each facade is responsible for:
1. Delegating HTTP calls to generated services.
2. Applying DTO-to-UI mapping.
3. Translating API errors into application error contracts.
4. Keeping component-facing APIs stable even if backend DTOs evolve.

## 6) Public API Surface

`core/api/index.ts` is the only entrypoint for feature modules. It re-exports:
- Facade classes
- API provider setup
- Shared error contracts

Generated classes may be re-exported only when strictly needed by facade internals or tests, not by feature presentation modules.

## 7) Regeneration Safety Contract

1. Regeneration may fully replace `core/api/generated/**`.
2. Handwritten code must live outside `generated/`.
3. Facade and mapper contracts are the stability layer protecting features from generated churn.
4. CI freshness checks (defined in the next subtask) validate that regenerated output is committed when OpenAPI contracts change.

## 8) Alignment With Existing Frontend Architecture

- Maintains feature-based module structure by exposing only facades to feature containers.
- Preserves smart/dumb component split by keeping transport/mapping out of presentation components.
- Supports auth interceptor flow because generated services are provided through Angular DI and shared HTTP pipeline.
- Avoids handwritten duplicate HTTP services by making generated contracts the only transport implementation source.
