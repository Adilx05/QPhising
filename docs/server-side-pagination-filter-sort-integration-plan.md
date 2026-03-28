# Server-side pagination/filter/sort integration plan

## Scope and objective

This plan standardizes server-driven table/list querying across implemented frontend modules.

It defines:

- canonical query contract (`page`, `size`, `sort`, `filters`),
- per-screen UI-control to query-parameter mapping,
- response metadata requirements (`totalCount`, page boundaries),
- URL query-state persistence and restore behavior.

All integrations must use generated OpenAPI clients wrapped by feature facades (no handwritten duplicate HTTP transport).

---

## Architecture constraints

- Feature components emit table interaction events (page/sort/filter/search) only.
- Feature facade owns query state and maps UI state to generated-client request DTOs.
- Generated client handles transport; facade maps DTOs to UI models.
- Shared mapper normalizes pagination metadata and empty-state semantics.

Canonical flow:

1. User changes page, sort, or filter from table controls.
2. Component emits a typed `ListQueryState` event.
3. Facade projects to endpoint-specific request parameters.
4. Generated client executes request through gateway base URL.
5. Facade updates feature store with `{ items, totalCount, page, size, sort, filters }`.
6. Router query params are synchronized for deep-link/reload continuity.

---

## Canonical frontend query model

Use this internal (UI/facade) model consistently:

- `page`: 1-based page index for UI state.
- `size`: page size (allowed set: `10 | 20 | 50 | 100`).
- `sort`:
  - `field`: feature-specific sortable field key.
  - `direction`: `asc | desc`.
- `filters`: record of typed filter values (enums/date/search/status/etc.).
- `searchTerm`: optional free-text term.

### URL persistence rules

Each list route persists query state in router query parameters:

- `page`, `size`, `sortField`, `sortDir`, plus feature-specific filters.
- On first entry without query params, apply feature defaults.
- On refresh/navigation-back, restore state from URL exactly.
- Invalid query values fall back to defaults and are replaced in URL.

---

## Backend response contract requirements

For server-side pagination correctness, list endpoints must return:

- `items`: current page records.
- `totalCount`: total records matching filters.
- `page` and `size` (or equivalent `skip/take` echoed values).
- optional `hasNext`/`hasPrevious` (derived values acceptable).

If endpoint currently returns non-paged arrays, backend enhancement is required before enabling server-side mode in UI.

---

## Per-screen contract matrix

## 1) Campaigns list (`/campaigns`)

- Current backend endpoint: `GET /api/v1/campaigns`
- Existing query params:
  - `statuses[]`
  - `templateTypes[]`
  - `startsOnOrAfter`
  - `endsOnOrBefore`
  - `skip`
  - `take`

### UI-to-query mapping

- UI `page` + `size` -> `skip = (page - 1) * size`, `take = size`
- UI status multi-select -> `statuses[]`
- UI template type multi-select -> `templateTypes[]`
- UI date range start -> `startsOnOrAfter`
- UI date range end -> `endsOnOrBefore`
- UI sort -> **gap** (no sort params currently exposed)

### Required backend parity changes

- Add deterministic sort contract:
  - `sortField` in allowlist (`name`, `status`, `templateType`, `startDate`, `endDate`, `createdAt`)
  - `sortDir` (`asc|desc`)
- Ensure response includes `totalCount` for paginator accuracy.

---

## 2) Templates list (`/templates`)

- Current backend endpoint: `GET /api/v1/templates`
- Existing query params:
  - `status`
  - `type`
  - `searchTerm`
  - `pageNumber`
  - `pageSize`

### UI-to-query mapping

- UI `page` -> `pageNumber`
- UI `size` -> `pageSize`
- UI status filter -> `status`
- UI template type filter -> `type`
- UI search input -> `searchTerm`
- UI sort -> **gap** (no sort params currently exposed)

### Required backend parity changes

- Add sortable query params (`sortField`, `sortDir`) with allowlisted fields.
- Ensure paged response contract always includes `totalCount` and page metadata.

---

## 3) Exports jobs list (`/exports`)

- Current backend capabilities:
  - `POST /api/v1/exports` (queue)
  - `GET /api/v1/exports/{exportJobId}` (detail)
- List endpoint for paged table is **missing**.

### Required backend endpoint

- Add `GET /api/v1/exports` with filters/paging/sort:
  - `page`, `size`, `sortField`, `sortDir`, `status`, `exportType`, `from`, `to`, `correlationId`
- Response must return paged contract with `totalCount`.

### Frontend mapping (target)

- paginator controls -> `page`, `size`
- status/exportType chips -> enum filters
- date range -> `from`, `to`
- correlation search -> `correlationId`
- column sort -> `sortField`, `sortDir`

---

## 4) Tracking events list (`/tracking`)

- Current backend capabilities:
  - tracking click-processing endpoint exists
- Paginated events list query endpoint is **missing**.

### Required backend endpoint

- Add `GET /api/v1/tracking/events` with:
  - `page`, `size`, `sortField`, `sortDir`, `campaignId`, `eventType`, `from`, `to`, `recipientEmail`

### Frontend mapping (target)

- table paginator -> `page`, `size`
- campaign selector -> `campaignId`
- event type filter -> `eventType`
- date window -> `from`, `to`
- recipient search -> `recipientEmail`
- column sort -> `sortField`, `sortDir`

---

## 5) Tasks queue list (`/tasks`)

- Current backend task list query endpoint for server-side table is **missing**.

### Required backend endpoint

- Add `GET /api/v1/tasks` with:
  - `page`, `size`, `sortField`, `sortDir`, `status`, `assignee`, `scheduledFrom`, `scheduledTo`, `searchTerm`

### Frontend mapping (target)

- paginator -> `page`, `size`
- status filter -> `status`
- assignee filter -> `assignee`
- schedule range -> `scheduledFrom`, `scheduledTo`
- search box -> `searchTerm`
- sort headers -> `sortField`, `sortDir`

---

## 6) Dashboard campaigns table (`/dashboard` widget)

- Dashboard list is currently fed by dashboard/campaign data integration plan.
- For scalable server mode, campaign sub-table must support paged query contract.

### Recommended options

- Preferred: use `GET /api/v1/campaigns` with dashboard filter-derived date/status criteria.
- Alternative: add dedicated `GET /api/v1/analytics/dashboard-campaigns` if server-side projection differs from campaign canonical model.

### UI-to-query mapping (target)

- global dashboard window filter drives date range params.
- local table search/status/sort/pagination drives list query params.
- route/query-state key namespace should avoid collisions with KPI/trend filters.

---

## Sort and filter normalization rules

- Sort field values must be backend allowlisted and case-sensitive by contract.
- `sortDir` must map PrimeNG sort order:
  - `1` -> `asc`
  - `-1` -> `desc`
- Empty filters are omitted from requests (not sent as empty strings).
- Multi-select filters send repeated query keys or arrays per generated-client encoding.
- Date filters serialize as ISO-8601 UTC (`toISOString()`).

---

## Query-state persistence contract

For each list route:

1. Read query params on initialization.
2. Validate/coerce values into typed `ListQueryState`.
3. Trigger data load using coerced state.
4. On state change, push updated query params via router navigation.
5. Preserve unrelated query params owned by parent feature filters.

State keys (baseline):

- `page`
- `size`
- `sortField`
- `sortDir`
- feature-specific filters (`status`, `type`, `search`, etc.)

---

## Delivery sequencing

1. Backend parity: add/complete missing list endpoints and sort/totalCount contracts.
2. Regenerate OpenAPI client and verify deterministic output.
3. Add shared `ListQueryState` + param mappers in frontend `core/api` layer.
4. Integrate templates + campaigns first (existing list endpoints).
5. Integrate exports/tracking/tasks when list endpoints become available.
6. Enable dashboard table server mode after contract decision (campaigns reuse vs dedicated analytics endpoint).

---

## Verification checklist

- Each list page emits backend requests with expected page/size/sort/filter params.
- Paginator total records match backend `totalCount`.
- Direct-link with query params restores identical view state.
- Back/forward navigation preserves list query state.
- Invalid URL query values are safely coerced and normalized.
- No feature component performs handwritten transport logic.

