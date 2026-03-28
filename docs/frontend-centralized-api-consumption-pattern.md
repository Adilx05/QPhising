# Frontend Centralized API Consumption Pattern

Last updated: 2026-03-28  
Owner: Codex  
Scope: `frontend/src/app/**` API integration boundaries and usage conventions.

## 1) Goal

Standardize frontend API consumption so every feature module calls backend APIs through a single, clean, generated-client-first architecture with:

- one transport path (generated OpenAPI client),
- consistent error translation,
- cancellation-by-default behavior,
- deterministic retry/backoff policy.

This pattern is mandatory for all new and migrated integrations.

## 2) Canonical architecture

## 2.1 Required request flow

`Container/Store -> Feature Facade -> Core API Gateway Wrapper -> Generated OpenAPI Service -> HTTP`

Response/error flow:

`HTTP -> Generated Service -> Core API Error Translator -> Facade result contract -> UI state`

## 2.2 Layer responsibilities

- **Containers/components**
  - Trigger feature intents only.
  - Consume typed UI models and view-state contracts.
  - Must not import generated DTO/service classes.

- **Feature facades** (`features/*/data-access`)
  - Orchestrate feature use cases.
  - Compose generated client calls via core wrappers.
  - Invoke feature mappers for DTO->UI normalization.
  - Convert translated API errors into feature state contracts.

- **Core API wrapper** (`core/api`)
  - Provide common call helpers for cancellation, retry rules, and normalized success/error envelopes.
  - Encapsulate shared transport-level policies; no feature business logic.

- **Generated OpenAPI services** (`core/api/generated`)
  - Exclusive source of HTTP operations.
  - Regenerated from backend OpenAPI; never manually edited.

## 2.3 Explicitly prohibited patterns

- Direct `HttpClient` calls inside feature components, containers, stores, or ad-hoc services duplicating generated operations.
- Feature-level retry implementations with inconsistent policies.
- Repeated inline `catchError` mapping logic scattered across components.
- Passing raw generated DTOs directly into templates.

## 3) Standard facade contract

Each feature facade operation should return one of:

- `Observable<UiModel>` for simple read flows, or
- `Observable<ApiCallResult<UiModel, ApiErrorModel>>` for flows needing explicit stateful error handling.

Recommended envelope:

- `ApiCallResult.success(data, metadata?)`
- `ApiCallResult.failure(error, metadata?)`

Where metadata may include request correlation id and retryability hints.

## 4) Shared error translation standard

## 4.1 Core translated error model

All transport/domain error responses must be translated into a shared model (example fields):

- `kind`: `validation | unauthorized | forbidden | notFound | conflict | rateLimited | transient | fatal`
- `message`: user-safe summary
- `detail`: optional technical detail for diagnostics
- `fieldErrors`: optional dictionary for form projection
- `traceId`: backend trace/correlation id when present
- `statusCode`: raw HTTP code
- `isRetryable`: boolean derived from policy

## 4.2 Translation rules

- `400` with validation dictionary -> `validation` + populated `fieldErrors`.
- `401` -> `unauthorized` (auth refresh/login workflow).
- `403` -> `forbidden` (policy mismatch UX).
- `404` -> `notFound`.
- `409` -> `conflict`.
- `429` -> `rateLimited` (retryable; obey `Retry-After` when present).
- `5xx` and network failures -> `transient` unless policy marks fatal.
- Schema/mapper faults -> `fatal` (programming/contract drift issue).

## 4.3 Projection ownership

- Core API layer translates HTTP errors into shared error model.
- Feature facades map shared errors into screen/form view-state.
- Components only render already-normalized error state.

## 5) Cancellation standard

## 5.1 Cancellation defaults

- Every in-flight read request tied to changing UI context (route param/filter/sort/search/tab change) must cancel previous request.
- Use RxJS cancellation patterns (`switchMap`, `takeUntilDestroyed`) in facade/store orchestration.

## 5.2 Cancellation ownership

- Facades/stores own cancellation orchestration.
- Generated client calls remain stateless and cancellation-agnostic.

## 5.3 Long-running operations

- For writes (create/update/archive/activate/schedule), do not auto-cancel after user submit unless user explicitly aborts.
- Expose explicit cancel intent only where operation semantics permit safe cancellation.

## 6) Retry and backoff policy

## 6.1 Default retry matrix

| Operation type | Auto retry | Max attempts | Backoff | Notes |
|---|---|---:|---|---|
| Idempotent reads (`GET`) | Yes (transient only) | 2 retries (3 total) | exponential + jitter | Skip retry for `401/403/404/409`. |
| Idempotent commands (domain-approved only) | Conditional | 1 retry | fixed short delay | Must be explicitly marked safe/idempotent. |
| Non-idempotent writes (`POST` create) | No auto retry | 0 | n/a | Offer manual retry UX only. |
| `429` rate limit | Yes | Respect server hint | `Retry-After` first, else exponential | Surface rate-limit state in UI. |

## 6.2 Retry placement

- Implement retry/backoff once in core API wrapper utilities.
- Feature code should request policy profile (`readDefault`, `writeNoRetry`, `rateLimitAware`) rather than handcrafting operators.

## 7) Generated-client-first implementation pattern

## 7.1 Required structure

- `frontend/src/app/core/api/generated/` -> generated services/models only.
- `frontend/src/app/core/api/facades/` -> optional cross-feature API coordinators.
- `frontend/src/app/core/api/errors/` -> shared error translation.
- `frontend/src/app/core/api/policies/` -> retry/cancellation policy helpers.
- `frontend/src/app/features/<feature>/data-access/` -> feature facade + mappers.

## 7.2 Integration steps for each endpoint

1. Regenerate OpenAPI client (`frontend/scripts/generate-openapi-client.sh`).
2. Verify generated operation and request/response types exist.
3. Add/extend feature facade method that wraps generated operation via core API wrapper.
4. Apply DTO->UI mapper and error translation.
5. Bind resulting observable contract to feature store/component state.

## 8) Feature adoption matrix

| Feature | Facade entrypoint | Generated service families | Required shared policies |
|---|---|---|---|
| Dashboard | `features/dashboard/data-access/dashboard.facade.ts` | analytics + campaigns read endpoints | `readDefault`, cancellation on filter change |
| Campaigns | `features/campaigns/data-access/campaigns.facade.ts` | campaigns CRUD + lifecycle | `readDefault`, `writeNoRetry`, validation error mapping |
| Templates | `features/templates/data-access/templates.facade.ts` | templates CRUD/publish/archive | `readDefault`, `writeNoRetry`, validation error mapping |
| Tracking | `features/tracking/data-access/tracking.facade.ts` | link generation/click events | `writeNoRetry`, rate-limit aware for click flows |
| Tasks | `features/tasks/data-access/tasks.facade.ts` | tasks list/detail/commands (as available) | `readDefault`, command policy per idempotency |
| Analytics | `features/analytics/data-access/analytics.facade.ts` | analytics summaries/trends | `readDefault`, cancellation on range change |
| Exports | `features/exports/data-access/exports.facade.ts` | queue/status/download | `writeNoRetry` for queue, `readDefault` for status polling |
| Auth/Core | `core/auth` and `core/api` | access probes/session endpoints | no retry for auth failures, strict unauthorized handling |

## 9) Governance and quality gates

A feature integration is complete only if all are true:

1. API calls route through generated client methods (no handwritten duplicate transport).
2. Shared error translation is used (no ad-hoc local HTTP error parsing).
3. Cancellation behavior is explicit for route/filter-driven reads.
4. Retry policy is selected from shared profiles, not custom per-component operators.
5. Facade output contracts are typed and UI-model based.
6. DTO-to-UI mappers are updated with any contract change.

## 10) Execution checklist

1. Add/confirm core API wrapper helpers for success/error envelopes.
2. Add/confirm shared error translator and error model.
3. Add/confirm shared retry policy helpers and rate-limit support.
4. Migrate each feature facade to call generated services via shared wrapper.
5. Remove/forbid duplicate handwritten API service code.
6. Add tests for error translation and retry/cancellation behavior in facade-level integration tests.
