# Frontend async UX state standard (loading / empty / error / retry)

## Scope and objective

This standard defines a shared asynchronous view-state model for all API-driven screens/components so every feature presents consistent:

- loading skeleton behavior,
- empty-state messaging,
- recoverable error handling,
- explicit retry actions.

It is designed for the current Angular feature structure (`dashboard`, `campaigns`, `templates`, `tracking`, `tasks`, `analytics`, `exports`) and must be used through feature facades + store-driven state updates.

## Architecture constraints

- Feature components **must not** perform handwritten transport calls.
- Feature containers consume facade/state signals and render UX states only.
- API transport remains in generated OpenAPI clients and facade wrappers.
- Error normalization must run through shared API error-mapping utilities.
- State transitions must be deterministic and traceable from one source of truth per feature state slice.

Canonical flow:

1. User interaction or route activation triggers facade `load*` action.
2. Facade sets feature state to `loading` with prior data preservation policy.
3. Generated client request executes.
4. Success path maps DTOs -> UI model and sets either `success` or `empty` state.
5. Failure path maps API/network errors to typed error state with retry metadata.
6. User-triggered retry replays the original request intent.

## Shared async state contract

## Type model

```ts
export type AsyncViewStatus =
  | 'idle'
  | 'loading'
  | 'success'
  | 'empty'
  | 'error-recoverable'
  | 'error-fatal';

export interface AsyncViewState<TData, TFilter = string> {
  status: AsyncViewStatus;
  data: TData;
  loading: boolean;
  errorMessage: string | null;
  errorCode: string | null;
  canRetry: boolean;
  retryLabel: string;
  activeFilter: TFilter;
  lastUpdatedUtc: string | null;
}
```

## Behavioral rules

- `loading`
  - `loading = true`, disable destructive/duplicate-trigger actions.
  - Render skeletons (cards/chart/table skeletons) instead of generic spinners where layout is known.
- `success`
  - `loading = false`, render data views.
  - Persist `lastUpdatedUtc` for diagnostics and stale-data messaging.
- `empty`
  - Used only when request succeeds and data collection/result is empty.
  - Show contextual empty message + primary CTA (refresh/reload or create action if authorized).
- `error-recoverable`
  - For transient failures (network timeout, 429, 5xx temporary upstream errors).
  - Must show retry button with preserved user filter context.
- `error-fatal`
  - For non-recoverable errors (`403` forbidden for requested action, contract violations, unsupported route).
  - Retry hidden by default unless a state change could resolve issue (e.g., refreshed auth).

## Transition matrix

| Current | Trigger | Next | Notes |
|---|---|---|---|
| `idle` | initial load | `loading` | Route init / explicit refresh. |
| `loading` | success + non-empty payload | `success` | Render feature data widgets. |
| `loading` | success + empty payload | `empty` | Empty state is successful response, not an error. |
| `loading` | transient failure | `error-recoverable` | Keep prior filter and attach retry action. |
| `loading` | non-recoverable failure | `error-fatal` | Show support/guidance message. |
| `error-recoverable` | retry clicked | `loading` | Same request intent and params. |
| `success/empty` | filter changed | `loading` | Optional stale snapshot may remain behind skeletons. |

## Error classification and retry policy

Recoverable errors (`error-recoverable`, `canRetry = true`):

- gateway timeout / network interruption,
- HTTP `429` rate-limited,
- HTTP `502/503/504` upstream availability issues.

Fatal errors (`error-fatal`, `canRetry = false` unless auth refresh path exists):

- HTTP `400` with validation/domain request defect,
- HTTP `403` authorization mismatch,
- HTTP `404` for immutable deleted/missing resource route,
- schema/contract parsing errors requiring code fix.

Retry rules:

- Prefer **manual retry** CTA.
- No unbounded auto-retry loops.
- Optional single delayed retry (max 1) may be used for dashboard read-only views if UX requires.

## UX rendering standard

## Loading

- KPI cards: card skeletons matching final dimensions.
- Charts: chart container skeleton + muted legend placeholders.
- Data tables: row skeletons with column-consistent widths.
- Detail pages: title/metadata skeleton + section skeleton blocks.

## Empty

Each empty state must include:

- concise contextual title,
- explanation sentence,
- primary action (`Refresh` or `Create` when role permits),
- optional secondary link to related module/docs.

## Error

Each error state must include:

- severity-specific banner style,
- user-safe message (no stack trace),
- correlation/trace id when available,
- retry button only when `canRetry = true`.

## Accessibility

- Use `aria-busy="true"` on loading regions.
- Error/empty banners must be announced with `aria-live="polite"`.
- Retry button keyboard focus order must follow banner text.

## Adoption checklist by screen

### Dashboard

- KPI cards use loading skeletons while dashboard KPI request is pending.
- Trend chart empty-state shown only when analytics response has zero points.
- Recoverable analytics errors surface retry preserving active date filter.

### Campaigns list/detail

- List table uses loading skeleton rows and standardized empty-state CTA.
- Detail route `/campaigns/:campaignId` handles `404` as fatal with navigation affordance back to list.
- Schedule/activate follow-up refresh uses same async-state transition rules.

### Templates list/detail

- List/detail share same async-state contract and empty copy tone.
- Archive action failures classify `403` as fatal and transport outages as recoverable.

### Tracking

- Tracking list uses empty-state for no events in selected range (not an error).
- Tracking-link generation failures map to form-level error + optional retry where idempotent.

### Tasks

- Read-only queue page still uses shared loading/empty/error-retry states.
- Missing backend capabilities must show deterministic informational empty/fatal copy, never placeholder arrays.

### Analytics

- Chart + table subregions must avoid mixed-state ambiguity; both derive from shared feature status.
- Retry action replays same filter window and chart granularity.

### Exports

- Jobs table empty-state includes “Queue export” CTA when role allows.
- Polling failures degrade to recoverable error with manual retry (resume polling) action.

## Component and state ownership standards

- Shared presentational components should be introduced/used for:
  - `AsyncLoadingStateComponent`
  - `AsyncEmptyStateComponent`
  - `AsyncErrorStateComponent`
- Feature containers provide inputs and callbacks (`onRetry`, optional `onPrimaryAction`).
- Store/facade remains authoritative for state mutation and retry intent reconstruction.

## Definition of done for compliance

A screen is compliant only if all are true:

1. Uses shared async status model (or equivalent fields) with explicit status value.
2. Distinguishes `empty` from `error` after successful response.
3. Implements recoverable error retry preserving filter/query context.
4. Uses no handwritten duplicate HTTP transport in component/container.
5. Presents accessible loading/error/empty regions.

## Verification commands (implementation support)

- Validate there is no direct transport leakage in feature layer:
  - `rg -n "HttpClient|http\\.(get|post|put|delete|patch)|fetch\\(|/api/" frontend/src/app/features frontend/src/app/shared -g '*.ts'`
- Locate current feature state shape for migration alignment:
  - `rg -n "FeatureViewState|loading: boolean|error: string" frontend/src/app/core/state/app-state.store.ts`
- Review dashboard template state branches:
  - `rg -n "viewState\(\)\.loading|viewState\(\)\.error|emptyDashboardState" frontend/src/app/features/dashboard/containers/dashboard-page.component.html`
