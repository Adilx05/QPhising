# Frontend Hardcoded/Mock Data Inventory

Last updated: 2026-03-28
Owner: Codex
Scope: `frontend/src/app/**`

## Inventory Table

| Module | File path | Component/Store | Mocked data source type | Evidence summary | Replacement priority |
|---|---|---|---|---|---|
| Core/Auth + Layout | `frontend/src/app/core/state/app-state.store.ts` | `AppStateStore.sessionState` | In-memory signal default session identity | Hardcoded `userId`, `displayName`, `email`, `role`, `authenticated` bootstrap user. | P0 |
| Dashboard | `frontend/src/app/core/state/app-state.store.ts` | `AppStateStore.dashboardKpisState` | In-memory signal array | Static KPI cards (`Total Campaigns`, `Clicks (24h)`, etc.). | P0 |
| Dashboard | `frontend/src/app/core/state/app-state.store.ts` | `AppStateStore.dashboardTrendState` | In-memory signal array | Static 7-day click trend points (`Mon`..`Sun`). | P0 |
| Dashboard | `frontend/src/app/core/state/app-state.store.ts` | `AppStateStore.dashboardCampaignsState` | In-memory signal array | Static campaign summary rows for dashboard table. | P0 |
| Dashboard + all features | `frontend/src/app/core/state/app-state.store.ts` | `AppStateStore.featureState` | In-memory signal defaults | Static loading/error/filter state for dashboard/campaigns/templates/tracking/tasks/analytics/exports. | P1 |
| Analytics | `frontend/src/app/features/analytics/containers/analytics-page.component.ts` | `AnalyticsPageComponent.kpis` | In-component literal array | Hardcoded KPI metrics (`Open Rate`, `Click Rate`, `Credential Submit`). | P0 |
| Analytics | `frontend/src/app/features/analytics/containers/analytics-page.component.ts` | `AnalyticsPageComponent.trendRows` | In-component literal array | Hardcoded weekly trend table rows. | P0 |
| Campaigns | `frontend/src/app/features/campaigns/containers/campaigns-page.component.ts` | `CampaignsPageComponent.campaigns` | In-component literal array | Hardcoded campaign list rows with template/status/owner. | P0 |
| Templates | `frontend/src/app/features/templates/containers/templates-page.component.ts` | `TemplatesPageComponent.templates` | In-component literal array | Hardcoded template list rows. | P0 |
| Tracking | `frontend/src/app/features/tracking/containers/tracking-page.component.ts` | `TrackingPageComponent.events` | In-component literal array | Hardcoded tracking events rows. | P0 |
| Tasks | `frontend/src/app/features/tasks/containers/tasks-page.component.ts` | `TasksPageComponent.tasks` | In-component literal array | Hardcoded queue/task processing rows. | P0 |
| Exports | `frontend/src/app/features/exports/containers/exports-page.component.ts` | `ExportsPageComponent.exportJobs` | In-component literal array | Hardcoded export job rows. | P0 |

## Priority Legend

- **P0**: Direct user-facing business data currently mocked; must be migrated to API-backed flow first.
- **P1**: UI state/config defaults that can remain locally managed but should be standardized once data-access layer is introduced.

## Notes

- The current frontend has no dedicated API data-access service layer under features; list/detail pages rely on in-component literals or `AppStateStore` signals initialized with static values.
- This inventory is strictly scoped to the “hardcoded/mock data usage” audit and intentionally does not yet prescribe endpoint mapping or client-generation strategy (covered by subsequent TASKS subtasks).
