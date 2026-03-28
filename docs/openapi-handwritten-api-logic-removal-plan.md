# Handwritten Duplicate API Logic Removal Plan

- Date: 2026-03-28
- Status: Approved
- Related task: `18.2 / Plan removal of handwritten duplicate API logic`
- Depends on:
  - `docs/frontend-mock-data-inventory.md`
  - `docs/frontend-backend-integration-gap-matrix.md`
  - `docs/openapi-generated-client-module-structure.md`

## 1) Objective

Define a safe, phased deprecation/removal sequence for handwritten API logic that duplicates (or would duplicate) generated OpenAPI clients, while preventing regressions in currently active modules.

## 2) Audit Scope and Evidence Baseline

### Scope scanned
- `frontend/src/app/**` TypeScript sources for:
  - direct `HttpClient` usage
  - `fetch(...)` usage
  - handwritten API service/facade classes implementing transport
  - direct `/api/*` endpoint string usage in feature/presentation layers

### Evidence summary
1. **No handwritten HTTP transport layer currently exists** in frontend source (no `HttpClient`, no `fetch`, no feature API service classes).
2. Current frontend business data flows are mock/static-backed (`AppStateStore` signals and in-component arrays), which represent **future replacement targets**, not direct duplicate transport code yet.
3. Generated-client architecture already defines `core/api/generated/**` as sole transport source with facades/mappers as handwritten adaptation layers.

Because direct manual transport duplicates are not yet present, this plan is **preventive + migration-governance focused** and targets replacement of mock/manual data providers with generated-client facades without introducing duplicate handwritten HTTP services.

## 3) Duplicate/Manual Logic Register and Replacement Mapping

| Legacy/manual source | Current location(s) | Duplicate risk category | Generated-client replacement target | Removal trigger |
|---|---|---|---|---|
| Dashboard mock KPI/trend/campaign data in store | `core/state/app-state.store.ts` | In-memory business data duplicate | `core/api/facades/analytics-api.facade.ts`, `campaigns-api.facade.ts` + mappers | Dashboard widgets fully sourced from live facade observables/signals |
| Campaigns list literal array | `features/campaigns/containers/campaigns-page.component.ts` | In-component manual data source duplicate | `campaigns-api.facade.ts` + campaign mapper | Campaign list page bound to generated campaign client-backed facade |
| Templates list literal array | `features/templates/containers/templates-page.component.ts` | In-component manual data source duplicate | `templates-api.facade.ts` + template mapper | Templates list page bound to generated templates client-backed facade |
| Analytics KPI/trend literal arrays | `features/analytics/containers/analytics-page.component.ts` | In-component manual data source duplicate | `analytics-api.facade.ts` + analytics mapper | Analytics page reads only facade-derived state |
| Tracking events literal array | `features/tracking/containers/tracking-page.component.ts` | In-component manual data source duplicate | `tracking-api.facade.ts` + tracking mapper (after backend list endpoint is available) | Tracking read endpoint available and wired through generated client |
| Tasks queue literal array | `features/tasks/containers/tasks-page.component.ts` | In-component manual data source duplicate | `tasks-api.facade.ts` + tasks mapper (after tasks API exists) | Tasks API endpoint available and wired through generated client |
| Exports job literal array | `features/exports/containers/exports-page.component.ts` | In-component manual data source duplicate | `exports-api.facade.ts` + exports mapper | Export queue/status flow uses generated exports endpoints only |
| Hardcoded session identity/role bootstrap | `core/state/app-state.store.ts` | Manual auth/session state duplicate | Auth bootstrap + API access policy probes via generated/facade layer where needed | Session state derived from real auth claims/session flow |

## 4) Staged Deprecation and Safe Removal Order

### Stage 0 — Guardrails before replacement
1. Keep `core/api/generated/**` as exclusive transport implementation area.
2. Forbid introducing new handwritten `HttpClient` feature services that overlap generated endpoints.
3. Require all feature integrations to consume only `core/api/facades/**`.

### Stage 1 — Replace P0 flows with available backend contracts
Order:
1. Dashboard (KPI/trend/campaign projection)
2. Campaigns list/detail
3. Templates list/detail
4. Analytics KPI/trend
5. Exports queue/status

For each feature in Stage 1:
1. Introduce facade + mapper.
2. Switch container components/store selectors to facade state.
3. Remove corresponding literal arrays/hardcoded in-memory business data.
4. Validate no remaining static data source in that feature path.

### Stage 2 — Replace currently blocked flows once backend contracts exist
Order:
1. Tracking table (after read/list endpoint contract exists in API/OpenAPI)
2. Tasks queue page (after tasks endpoint contract exists in API/OpenAPI)

For each Stage 2 feature:
1. Add backend endpoint + OpenAPI surface first.
2. Regenerate client.
3. Implement facade+mapper wiring.
4. Remove temporary literal arrays from container/store.

### Stage 3 — Auth/session duplicate cleanup
1. Move role/user/session ownership to real auth claim/session bootstrap pipeline.
2. Remove hardcoded `sessionState` identity defaults once Keycloak-backed flow is live.
3. Keep UI-only view state (loading/error/filter) local, but remove business-data defaults.

## 5) Safe Removal Checklist (Per Feature)

Before deleting any manual source:
1. Generated endpoint contract exists and compiles.
2. Facade wrapper exists and returns typed UI model(s) via mapper.
3. Loading/empty/error states are preserved.
4. Route guards/role checks use authoritative auth/session source.
5. No remaining imports/references to removed literal/mock provider.

After deletion:
1. Verify no static arrays remain for that feature’s business data.
2. Run frontend build/lint/tests applicable to the touched scope.
3. Run OpenAPI freshness check to ensure generated artifacts are in sync.

## 6) Anti-Regression Rules

1. `core/api/generated/**` remains generator-owned and non-editable by hand.
2. Handwritten code is allowed only in:
   - `core/api/client/**`
   - `core/api/facades/**`
   - `core/api/mappers/**`
   - `core/api/errors/**`
3. Feature containers/components must not call endpoint URLs directly.
4. Any new API integration PR must include:
   - generated client usage proof,
   - deletion (or explicit retirement plan) of replaced mock/manual source,
   - freshness-check pass output.

## 7) Expected End State

1. No business-data literals remain in feature containers or shared app-state bootstrap.
2. No handwritten transport services duplicate generated endpoint contracts.
3. All API consumption paths are facade-based, auth-aware, and mapper-normalized.
