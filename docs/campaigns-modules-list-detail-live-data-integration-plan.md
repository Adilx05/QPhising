# Campaigns/Modules List-Detail Live Data Integration Plan

- Date: 2026-03-28
- Status: Approved
- Related task: `18.3 / Plan campaigns/modules/list/detail live data integration`
- Scope:
  - Frontend: `frontend/src/app/features/**`
  - Backend: `backend/API/Controllers/**`

## 1) Objective

Define deterministic API-backed data flow for all module list/detail pages that are currently static, with explicit route-parameter detail retrieval rules and generated-client-first consumption boundaries.

## 2) Current-state findings

- Every feature module currently exposes only list-style routes (`path: ''`) and relies on in-component static arrays.
- No feature currently wires list/detail retrieval through generated OpenAPI clients.
- Backend supports list+detail for campaigns, templates, and exports status-by-id; tracking/tasks list/detail read endpoints are not available yet.

## 3) Route and endpoint coverage matrix (authoritative)

| Feature module | Current route(s) | Planned detail route (param contract) | List endpoint mapping | Detail endpoint mapping | Contract status |
|---|---|---|---|---|---|
| Campaigns | `/campaigns` | `/campaigns/:campaignId` (`campaignId: Guid`) | `GET /api/v1/campaigns?statuses&templateTypes&startsOnOrAfter&endsOnOrBefore&skip&take` | `GET /api/v1/campaigns/{campaignId}` | Available |
| Templates | `/templates` | `/templates/:templateId` (`templateId: Guid`) | `GET /api/v1/templates?status&type&searchTerm&pageNumber&pageSize` | `GET /api/v1/templates/{templateId}` | Available |
| Tracking | `/tracking` | `/tracking/:campaignId/events` (`campaignId: Guid`) | **Blocked**: no tracking-events list read endpoint exposed today | **Blocked**: no tracking-event detail/read endpoint exposed today | Missing backend read contracts |
| Tasks | `/tasks` | `/tasks/:taskId` (`taskId: Guid`) | **Blocked**: no `/api/tasks` list endpoint exposed today | **Blocked**: no task detail endpoint exposed today | Missing backend read contracts |
| Analytics | `/analytics` | `/analytics/campaign/:campaignId` (`campaignId: Guid`) | `GET /api/v1/analytics/dashboard-kpis?from&to&timeGrain&timeZone&campaignIds...` | Same endpoint, scoped by `campaignIds=[campaignId]` | Partially available (derived detail) |
| Exports | `/exports` | `/exports/:exportJobId` (`exportJobId: Guid`) | `GET /api/v1/exports/{exportJobId}` via active-jobs polling list strategy (aggregate client-side) | `GET /api/v1/exports/{exportJobId}` (+ optional `GET /download`) | Partially available (no native list endpoint) |

## 4) Feature-by-feature list/detail integration definitions

### 4.1 Campaigns module

- **List flow**
  - UI route: `/campaigns`
  - Query source: list filters + pagination controls.
  - API call: `CampaignsFacade.listCampaigns(criteria)` wrapping generated campaigns client.
  - Mapper binding:
    - DTO -> `CampaignListRowVm` fields: `id`, `name`, `templateTypeLabel`, `statusLabel`, `startDateText`, `endDateText`.
- **Detail flow**
  - UI route: `/campaigns/:campaignId`
  - Route binding: `campaignId` parsed from `ActivatedRoute.paramMap`.
  - API call: `CampaignsFacade.getCampaignById(campaignId)`.
  - Mapper binding:
    - DTO -> `CampaignDetailVm` fields: `id`, `name`, `templateType`, `status`, `htmlContent`, `window`, `auditMeta`.

### 4.2 Templates module

- **List flow**
  - UI route: `/templates`
  - API call: `TemplatesFacade.listTemplates(criteria)`.
  - Mapper binding:
    - DTO -> `TemplateListRowVm`: `id`, `name`, `typeLabel`, `statusLabel`, `updatedAtText`.
- **Detail flow**
  - UI route: `/templates/:templateId`
  - API call: `TemplatesFacade.getTemplateById(templateId)`.
  - Mapper binding:
    - DTO -> `TemplateDetailVm`: `id`, `name`, `type`, `status`, `htmlContent`, `variables[]`.

### 4.3 Tracking module

- **List flow**
  - UI route: `/tracking`
  - Current backend status: no read/list endpoint for tracking events.
  - Required backend contract to unblock:
    - `GET /api/v1/tracking/events?campaignId&from&to&pageNumber&pageSize` (proposed).
- **Detail flow**
  - UI route: `/tracking/:campaignId/events`
  - Current backend status: no detail/read contract.
  - Required backend contract to unblock:
    - `GET /api/v1/tracking/events/{eventId}` or campaign-scoped equivalent (proposed).

### 4.4 Tasks module

- **List flow**
  - UI route: `/tasks`
  - Current backend status: no tasks controller/read endpoint.
  - Required backend contract to unblock:
    - `GET /api/v1/tasks?status&type&pageNumber&pageSize` (proposed).
- **Detail flow**
  - UI route: `/tasks/:taskId`
  - Required backend contract:
    - `GET /api/v1/tasks/{taskId}` (proposed).

### 4.5 Analytics module

- **List/summary flow**
  - UI route: `/analytics`
  - API call: `AnalyticsFacade.getDashboardKpis(rangeCriteria)`.
  - Mapper binding:
    - DTO -> `AnalyticsKpiVm[]`, `AnalyticsTrendRowVm[]`.
- **Detail flow (campaign-scoped analytics)**
  - UI route: `/analytics/campaign/:campaignId`
  - API call: same analytics endpoint with `campaignIds=[campaignId]` and date window.
  - Mapper binding:
    - DTO -> `AnalyticsCampaignDetailVm` with trend and conversion breakdown.

### 4.6 Exports module

- **List flow**
  - UI route: `/exports`
  - Available contracts: queue export (`POST`) and status-by-id (`GET /{id}`), but no native list.
  - Integration rule:
    - Keep a client-side active export registry of IDs created by current session and hydrate rows by polling `GET /api/v1/exports/{id}`.
- **Detail flow**
  - UI route: `/exports/:exportJobId`
  - API call: `ExportsFacade.getExportStatus(exportJobId)` and optional download when completed.
  - Mapper binding:
    - DTO -> `ExportDetailVm`: `id`, `type`, `format`, `status`, `requestedBy`, `createdAt`, `completedAt`, `downloadReady`.

## 5) Standard route-param to API binding rules

1. Detail routes MUST parse IDs from URL params and validate GUID format before API invocation.
2. Invalid route param -> feature-level `invalid identifier` state (no API call).
3. Valid route param + 404 -> deterministic `not found` state.
4. Feature container remains dumb about transport; all API calls go through facade wrappers over generated clients.

## 6) Integration sequencing (list/detail)

1. Campaigns list -> campaigns detail.
2. Templates list -> templates detail.
3. Exports list strategy (session registry + status polling) -> export detail.
4. Analytics summary -> analytics campaign detail.
5. Tracking list/detail after backend read endpoint delivery.
6. Tasks list/detail after backend API delivery.

## 7) Acceptance criteria

- Every feature module has explicit list/detail endpoint mapping or a documented backend contract gap.
- Every detail flow specifies route-param binding and retrieval behavior.
- All mappings enforce generated-client-first architecture with facade + mapper separation.
