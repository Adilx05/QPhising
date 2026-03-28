# Backend Endpoint Utilization Report (Frontend Coverage)

Last updated: 2026-03-28  
Owner: Codex  
Scope: Implemented frontend feature pages in `frontend/src/app/**` cross-referenced with API controllers in `backend/API/Controllers/**`.

## Classification rules

- **used**: endpoint is consumed by current frontend code.
- **unused**: endpoint exists but no current frontend call path uses it.
- **not-applicable**: endpoint is infra/public callback oriented and not expected to be consumed directly by authenticated SPA feature pages.

## Evidence baseline

- No `HttpClient`, `fetch`, or `/api/*` usage exists in current frontend implementation.
- Frontend feature pages currently render in-memory/static data.

## Endpoint utilization matrix (UI-relevant)

| Backend endpoint | Method | Current utilization | Candidate frontend integration target | Priority | Notes |
|---|---|---|---|---|---|
| `/api/analytics/dashboard-kpis` | GET | unused | Dashboard KPI cards, dashboard trend chart/table, analytics summary widgets | P0 | Single analytics feed can power multiple UI modules with query filters/timegrain.
| `/api/campaigns` | GET | unused | Campaigns list page + dashboard campaigns widget | P0 | Required for replacing static campaign rows and list views.
| `/api/campaigns/{campaignId}` | GET | unused | Campaign detail route (future), row drill-down, edit prefill | P1 | UI has no detail route yet; endpoint is available for upcoming list->detail flow.
| `/api/campaigns` | POST | unused | Campaign creation form submission | P1 | Frontend currently has no create form wiring.
| `/api/campaigns/{campaignId}` | PUT | unused | Campaign edit form submission | P1 | Requires detail/edit UI to be wired.
| `/api/campaigns/{campaignId}/schedule` | POST | unused | Campaign row action (schedule) | P1 | Candidate contextual action in campaigns table.
| `/api/campaigns/{campaignId}/activate` | POST | unused | Campaign row action (activate) | P1 | Candidate contextual action in campaigns table.
| `/api/templates` | GET | unused | Templates list page | P0 | Needed to replace hardcoded templates list.
| `/api/templates/{templateId}` | GET | unused | Template detail route (future) | P1 | Backend ready; UI detail not implemented yet.
| `/api/templates` | POST | unused | Template creation form | P1 | Candidate once template CRUD views are added.
| `/api/templates/{templateId}` | PUT | unused | Template edit form | P1 | Candidate once edit workflow exists.
| `/api/templates/{templateId}/publish` | POST | unused | Template row action (publish) | P1 | Candidate table row operation.
| `/api/templates/{templateId}/archive` | POST | unused | Template row action (archive) | P1 | Candidate table row operation.
| `/api/exports` | POST | unused | Exports page “request export” action | P0 | Required to move from static export jobs to real queueing.
| `/api/exports/{exportJobId}` | GET | unused | Exports jobs polling/status refresh | P0 | Needed for live job status lifecycle in exports table.
| `/api/exports/{exportJobId}/download` | GET | unused | Exports table download action for completed jobs | P1 | Needed after queue/status integration.
| `/api/tracking/links` | POST | unused | Campaign workflow action to generate recipient tracking links | P1 | Relevant to campaign/tracking operations; current tracking page is read-table only.
| `/api/access/admin` | GET | unused | Auth/session bootstrap + policy verification for admin-role capabilities | P1 | Could support role diagnostics; primary role source should be token claims.
| `/api/access/operator` | GET | unused | Auth/session bootstrap + policy verification for operator-role capabilities | P1 | Same as above.
| `/api/access/viewer` | GET | unused | Auth/session bootstrap + policy verification for viewer-role capabilities | P1 | Same as above.

## Endpoint observations for implemented pages

1. **Dashboard/Campaigns/Templates/Analytics/Exports are backend-supported but frontend-unconsumed** (all required endpoint families exist, usage is currently zero).
2. **Tracking page mismatch**: frontend shows an events list table, but backend does not expose a corresponding read endpoint for listing tracking events. Current backend tracking endpoints are for link generation and click callback processing.
3. **Tasks page mismatch**: frontend has a queue table but backend API has no `/api/tasks` controller/endpoints to support the page.

## Non-UI or indirectly UI-relevant endpoints

| Endpoint | Method | Classification | Rationale |
|---|---|---|---|
| `/api/tracking/click/{campaignId}/{trackingToken}` | GET | not-applicable | Public callback endpoint for phishing-link click processing, not a standard authenticated SPA data query.
| `/api/health` | GET | not-applicable | Operational health probe endpoint for diagnostics/orchestration.

## Summary

- **Used UI-relevant endpoints:** 0
- **Unused UI-relevant endpoints:** 20
- **Primary frontend integration candidates (P0):** analytics dashboard-kpis, campaigns list, templates list, exports queue/status.
- **Backend capability gaps impacting implemented UI pages:** tracking events read endpoint and tasks queue API surface.
