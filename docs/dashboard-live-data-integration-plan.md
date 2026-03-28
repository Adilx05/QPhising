# Dashboard Live Data Integration Plan

- Date: 2026-03-28
- Status: Approved
- Related task: `18.3 / Plan dashboard live data integration`
- Scope: `frontend/src/app/features/dashboard/**`, `frontend/src/app/core/state/**`, backend dashboard/campaign read endpoints

## 1) Goal

Replace all dashboard hardcoded values with API-backed data via generated OpenAPI clients, while preserving clean architecture boundaries:

- Feature container/presentation components remain transport-agnostic.
- All HTTP transport is consumed through `core/api/facades/**` wrappers over generated clients.
- DTO-to-UI transformation is handled in `core/api/mappers/**`.

## 2) Current-state findings (hardcoded paths to remove)

1. Dashboard KPIs are currently hardcoded in `AppStateStore.dashboardKpisState`.
2. Dashboard trend points are currently hardcoded in `AppStateStore.dashboardTrendState`.
3. Dashboard campaigns table rows are currently hardcoded in `AppStateStore.dashboardCampaignsState`.
4. Dashboard filter changes currently only mutate local state (`updateFeatureFilter`) and do not trigger backend reads.

## 3) Authoritative backend endpoints and contracts

### 3.1 KPI + trend source

- Endpoint: `GET /api/v1/analytics/dashboard-kpis` (also exposed as unversioned `/api/analytics/dashboard-kpis` for compatibility).
- Query inputs required by dashboard integration:
  - `from` (DateTimeOffset, required)
  - `to` (DateTimeOffset, required)
  - `timeGrain` (Hour|Day|Week|Month)
  - `timeZone` (string, default `UTC`)
  - optional filter dimensions: `campaignIds`, `templateTypes`, `campaignStatuses`
- Response includes:
  - KPI groups: `campaigns`, `clicks`, `conversions`, `taskThroughput`
  - trend series: `trend[]`
  - additional breakdown datasets (`campaignStatusBreakdown`, `templateTypeBreakdown`)

### 3.2 Dashboard campaigns table source

- Endpoint: `GET /api/v1/campaigns` (also unversioned `/api/campaigns`).
- Query inputs for dashboard usage:
  - `statuses[]` (optional)
  - `templateTypes[]` (optional)
  - `startsOnOrAfter` (optional)
  - `endsOnOrBefore` (optional)
  - `skip` / `take`
- Response item fields available for dashboard row projection:
  - `id`, `name`, `templateType`, `startDate`, `endDate`, `status`

> Note: current campaign list response does not include `owner` or `clickRate`; these columns must be replaced or marked unsupported until a dedicated dashboard-campaigns endpoint exists.

## 4) Widget integration matrix (widget -> endpoint -> mapper -> UI contract)

| Dashboard widget | Endpoint | Generated service/facade method | Mapper output contract | UI state binding |
|---|---|---|---|---|
| KPI card: Total Campaigns | `GET /analytics/dashboard-kpis` | `AnalyticsApiFacade.getDashboardSnapshot(filter)` | `DashboardKpiCardVm { title: 'Total Campaigns', value: string }` from `campaigns.total` | `dashboard.data.kpis[]`, with loading/error/empty states |
| KPI card: Clicks (window) | `GET /analytics/dashboard-kpis` | same as above | `DashboardKpiCardVm { title: 'Clicks', value: string }` from `clicks.total` | same |
| KPI card: Conversion Rate | `GET /analytics/dashboard-kpis` | same as above | `DashboardKpiCardVm { title: 'Conversion Rate', value: percent }` from `conversions.conversionRatePercent` | same |
| KPI card: Tasks Queued/Processed | `GET /analytics/dashboard-kpis` | same as above | `DashboardKpiCardVm { title: 'Tasks Processed', value: string }` from `taskThroughput.processed` (or keep label configurable) | same |
| Trend chart (bars/line) | `GET /analytics/dashboard-kpis` | same as above | `DashboardTrendPointVm[] { bucketLabel, clicks, conversions, tasksProcessed, conversionRatePercent }` from `trend[]` | `dashboard.data.trend[]`; show empty state if no points |
| Trend detail table | `GET /analytics/dashboard-kpis` | same as above | `DashboardTrendRowVm[]` derived from trend VMs for table formatting | `dashboard.data.trendRows[]` |
| Campaigns table | `GET /campaigns` (+ optional follow-up analytics enrichment when available) | `CampaignsApiFacade.listCampaigns(criteria)` | `DashboardCampaignRowVm[] { name, status, templateType, startDate, endDate }` (owner/clickRate deferred) | `dashboard.data.campaignRows[]`; paged slice (top N) |

## 5) Filter-to-query translation rules

Dashboard date filter options must deterministically translate to analytics query windows (all in UTC unless user timezone provided):

- `Last 24 hours` -> `from = now - 24h`, `to = now`, `timeGrain = Hour`
- `Last 7 days` -> `from = now - 7d`, `to = now`, `timeGrain = Day`
- `Last 30 days` -> `from = now - 30d`, `to = now`, `timeGrain = Day`

Additional mapper rules:

- `timeZone` defaults to `UTC` until explicit user preference is available.
- Percent values use `toFixed(1)` for KPI cards and `toFixed(2)` for drilldown where needed.
- Enum presentation mapping:
  - `CampaignStatus` -> title case labels
  - `TemplateType` -> user-friendly labels (`Credential Harvest`, `Attachment`, `Landing Page`)
- Null-safe defaults:
  - missing numeric fields -> `0`
  - missing arrays -> `[]`
  - missing strings -> `'-'`

## 6) UI async state contract for dashboard container

`DashboardPageComponent` should consume one view model stream/signal from a facade/store adapter with explicit shape:

```ts
interface DashboardPageVm {
  loading: boolean;
  error: string | null;
  activeFilter: 'Last 24 hours' | 'Last 7 days' | 'Last 30 days';
  kpis: DashboardKpiCardVm[];
  trend: DashboardTrendPointVm[];
  trendRows: DashboardTrendRowVm[];
  campaignRows: DashboardCampaignRowVm[];
  lastUpdatedUtc: string | null;
}
```

State transitions:

1. `applyFilter()` sets `loading=true`, clears stale errors, invokes analytics/campaign facades.
2. On success: map DTOs, update all widget VMs atomically, set `loading=false`.
3. On failure: preserve last good data, set normalized error message, set `loading=false`.
4. Retry action replays last request criteria.

## 7) Generated-client first implementation path

1. Generate API client from authoritative `openapi/v1.json`.
2. Add/extend facades (no feature-level `HttpClient`):
   - `core/api/facades/analytics-api.facade.ts`
   - `core/api/facades/campaigns-api.facade.ts`
3. Add mappers:
   - `core/api/mappers/dashboard.mapper.ts` (analytics -> KPI/trend/table VMs)
   - `core/api/mappers/campaigns.mapper.ts` (campaign list -> dashboard rows)
4. Replace hardcoded dashboard signal seeds with facade-driven load effect in dashboard container/store adapter.
5. Keep `AppStateStore` responsible for generic view-state orchestration only (not static business datasets).

## 8) Risks and mitigations

- Risk: campaigns table parity gap (`owner`, `clickRate` absent in campaign list contract).
  - Mitigation: temporarily adjust table columns to contract-backed fields and log a backend enhancement follow-up for enrichment endpoint/fields.
- Risk: analytics endpoint requires valid `from < to` and can return `400`.
  - Mitigation: centralized filter mapper guarantees valid windows; error mapper normalizes problem-details to user-facing message.
- Risk: timezone mismatches in trend bucket labels.
  - Mitigation: pass explicit timezone when available; include UTC fallback badge in UI metadata.

## 9) Acceptance criteria for this plan

1. Every current dashboard widget has an explicit endpoint + facade + mapper + UI contract mapping.
2. All hardcoded dashboard values have a documented replacement path.
3. Integration approach uses generated clients/facades, not handwritten duplicate transport services.
4. Known backend contract gaps are explicitly identified with mitigation.
