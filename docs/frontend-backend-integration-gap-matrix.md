# Frontend-to-Backend Integration Gap Matrix

Last updated: 2026-03-28  
Owner: Codex  
Scope: `frontend/src/app/**` cross-referenced with `backend/API/Controllers/**`

## Method

- Used the hardcoded/mock inventory (`docs/frontend-mock-data-inventory.md`) as the audited UI-flow baseline.
- Verified current frontend transport usage (`HttpClient`, `fetch`, `/api/*` references).
- Mapped each UI flow to the expected backend endpoint family currently available in API controllers.
- Classified status as:
  - `connected`: real API call exists in frontend and is wired to the flow.
  - `partially connected`: some API wiring exists, but flow still depends on local mock/stub logic.
  - `missing`: no frontend API wiring exists for the flow.

## Gap Matrix

| UI flow | Frontend owner | Expected backend endpoint(s) | Current status | Gap evidence | Integration priority |
|---|---|---|---|---|---|
| Dashboard KPI cards (`Total Campaigns`, `Clicks`, `Conversion Rate`, `Tasks Queued`) | `AppStateStore.dashboardKpisState` + `DashboardPageComponent.kpis` | `GET /api/analytics/dashboard-kpis` | missing | KPI data is in in-memory signals; no HTTP/API client usage in dashboard/core state. | P0 |
| Dashboard click trend chart + rows | `AppStateStore.dashboardTrendState` + `DashboardPageComponent.trend/trendRows` | `GET /api/analytics/dashboard-kpis` (time grain + trend) | missing | Trend points are hardcoded in store; no frontend request pipeline exists. | P0 |
| Dashboard campaigns table | `AppStateStore.dashboardCampaignsState` + `DashboardPageComponent.campaignRows` | `GET /api/campaigns` (filtered/top rows projection) | missing | Campaign rows are static in store; no query wiring to campaigns endpoint. | P0 |
| Campaigns list page | `CampaignsPageComponent.campaigns` | `GET /api/campaigns` | missing | Page uses in-component literal array; no API consumption path. | P0 |
| Templates list page | `TemplatesPageComponent.templates` | `GET /api/templates` | missing | Page uses in-component literal array; no API consumption path. | P0 |
| Analytics summary cards + trend table | `AnalyticsPageComponent.kpis/trendRows` | `GET /api/analytics/dashboard-kpis` | missing | Analytics values are static literals; no backend integration in feature module. | P0 |
| Tracking events table (operator-facing audit view) | `TrackingPageComponent.events` | **No dedicated list endpoint currently exposed** (`TrackingController` exposes only `POST /api/tracking/links` and public click endpoint) | missing | Frontend view exists but backend list/read model endpoint is absent for this UI flow. | P0 |
| Tasks queue table | `TasksPageComponent.tasks` | **No tasks API controller currently exposed** | missing | Frontend has static task rows; backend API surface has no `/api/tasks` controller. | P0 |
| Exports jobs table | `ExportsPageComponent.exportJobs` | `POST /api/exports`, `GET /api/exports/{id}` (+ optional download) | missing | Frontend renders static export jobs and does not call queue/status endpoints. | P0 |
| Session/auth identity used for role gating and nav | `AppStateStore.sessionState`, `routeAccessGuard`, `LayoutShellComponent` | `GET /api/access/admin|operator|viewer` (policy probes) and Keycloak session/token claims | missing | Role/auth state is hardcoded in store and not hydrated from token/session backend flow. | P0 |
| Feature loading/error/filter view states | `AppStateStore.featureState` | N/A (UI state orchestration, not a direct endpoint) | partially connected | Local state structure is useful, but it is not currently driven by real async API lifecycle because no endpoints are called. | P1 |

## Summary

- **Connected flows:** 0
- **Partially connected flows:** 1 (`featureState` shell only)
- **Missing flows:** 10

## Immediate implications for next execution tasks

1. Frontend has **zero live API integration** at present; all business data modules remain mock-backed.
2. Two UI modules (`Tracking`, `Tasks`) are currently blocked not only by frontend wiring but also by **missing backend read endpoints** for their presented tables.
3. Upcoming OpenAPI-client subtasks should treat this matrix as the source for migration sequencing (Dashboard/Campaigns/Templates/Analytics/Exports first, then Tracking/Tasks once backend contracts exist).
