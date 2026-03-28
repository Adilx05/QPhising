# DTO and Client Model Alignment Checklist

Last updated: 2026-03-28  
Owner: Codex  
Scope: UI-relevant backend contracts (`backend/Application/**`, `backend/API/Controllers/**`) vs current frontend models/view data (`frontend/src/app/**`).

## Audit method

1. Enumerated backend DTO contracts used by UI-relevant endpoints (analytics, campaigns, templates, exports, tracking, access).
2. Enumerated frontend state/view models currently shaping rendered data.
3. Compared for:
   - field presence and naming,
   - type compatibility (enum/domain/date/number/string),
   - nullability and optionality,
   - date-time serialization assumptions.
4. Assigned mapping ownership to feature-level data-access facades wrapping generated OpenAPI clients.

## Alignment status by feature

| Feature | Backend contract(s) | Current frontend model(s) | Status | Key mismatches | Mapping owner |
|---|---|---|---|---|---|
| Dashboard KPIs + trend | `DashboardKpisResponse`, `CampaignKpiSummary`, `ClickKpiSummary`, `ConversionKpiSummary`, `TaskThroughputKpiSummary`, `AnalyticsTrendPoint` | `DashboardKpi`, `DashboardTrendPoint`, `DashboardTrendRow` | mismatch | Backend uses structured numeric KPI groups + `BucketStartUtc`; frontend expects pre-formatted title/value strings and weekday labels. | `frontend/src/app/features/dashboard/data-access/dashboard.facade.ts` (to be added) |
| Dashboard campaigns widget | `ListCampaignsResponse.Items[]` (`CampaignListItemResponse`) | `DashboardCampaignRow` | mismatch | Backend item has `Id`, `TemplateType`, `StartDate/EndDate` (`DateTimeOffset`), no `owner`/`clickRate`; frontend requires `owner` and `clickRate` not present in contract. | `dashboard.facade.ts` + optional aggregation endpoint decision |
| Campaigns list | `ListCampaignsResponse` + `CampaignListItemResponse` | in-component literal `{ name, template, status, owner }` | mismatch | Field name mismatch (`template` vs `TemplateType`), missing `Id`, missing date fields in UI row, `owner` absent in backend list response. | `frontend/src/app/features/campaigns/data-access/campaigns.facade.ts` (to be added) |
| Campaign detail/edit | `CampaignDetailResponse` + create/update requests | no dedicated typed UI model | missing model | No frontend detail model exists; required fields include `HtmlContent`, `StartDate`, `EndDate`, `Status`, `TemplateType`. | `campaigns.facade.ts` + feature mapper |
| Templates list | `ListTemplatesResponse.Items[]` (`TemplateListItemResponse`) | in-component literal `{ name, type, quality, owner }` | mismatch | Backend uses `Status` + `Version` + `Variables`; frontend uses `quality`/`owner` fields that do not exist; enum domain differs. | `frontend/src/app/features/templates/data-access/templates.facade.ts` (to be added) |
| Template detail/edit | `TemplateDetailResponse` + create/update requests | no dedicated typed UI model | missing model | UI has no detail/edit models for `HtmlContent`, `Variables[]`, `Version`. | `templates.facade.ts` + feature mapper |
| Analytics page summary | `DashboardKpisResponse` | in-component literal KPI/trend arrays | mismatch | Analytics cards currently string percentages; backend provides numeric decimal percentages and richer breakdown arrays. | `frontend/src/app/features/analytics/data-access/analytics.facade.ts` (to be added) |
| Exports jobs | `ExportJobContract`, queue/status/download requests | in-component literal `{ report, format, status, requestedBy }` | mismatch | Backend has lifecycle timestamps, nullable file metadata, enum `ExportType`; frontend missing IDs and status timestamps needed for polling/download. | `frontend/src/app/features/exports/data-access/exports.facade.ts` (to be added) |
| Tracking operations | `GenerateTrackingLinkApiRequest/Response`, `ProcessTrackingClickApiResponse` | in-component literal events table | partially alignable | Current UI table needs event-list/read DTO not present in backend; existing backend DTOs are for link generation/click callback processing only. | `frontend/src/app/features/tracking/data-access/tracking.facade.ts` + backend contract extension |
| Auth/role gating | Access probe responses + Keycloak JWT claims | `SessionState`, `AppRole` | mismatch | Frontend session is hardcoded; lacks claim-driven mapping for `sub`, `preferred_username`, `email`, and role arrays. | `frontend/src/app/core/auth/auth-session.facade.ts` (to be added) |
| Tasks queue page | N/A (no API contract currently exposed) | in-component literal task rows | blocked | No backend DTO/endpoint exists for tasks queue read model; frontend model cannot align until backend adds contract. | Backend API + future `tasks.facade.ts` |

## Concrete mismatch checklist

### 1) Enum/value mismatches

- `TemplateType` mismatch is severe:
  - Backend campaign/template enums: `Email`, `LandingPage` (and campaign also `Sms`).
  - Frontend literals include `CredentialHarvest`, `InvoiceFraud`, `PasswordReset`, `Attachment`.
- `CampaignStatus` frontend row type omits backend `Archived` status.
- Exports format casing differs (`Pdf` in backend enum vs `PDF` label in UI literals).

### 2) Field name/shape mismatches

- Campaigns frontend uses `template`; backend uses `TemplateType`.
- Templates frontend uses `quality`; backend uses `Status`.
- Dashboard campaign widget expects `owner` and `clickRate`, absent in `CampaignListItemResponse`.
- Exports frontend expects `report`/`requestedBy`; backend uses `ExportType`/`OwnerUserId` plus IDs and timestamps.

### 3) Type and nullability mismatches

- Backend uses `DateTimeOffset` (ISO timestamps); frontend currently stores display-ready strings.
- Backend export contract has many nullable fields (`CompletedAt`, `FileName`, `ErrorMessage`, etc.); frontend model has no nullable handling.
- Backend analytics returns decimals and longs; frontend currently stores formatted strings with `%`.

### 4) Missing frontend models

- No typed models exist for campaign/template detail flows.
- No typed auth/session DTO-to-view model mapper exists.
- No tasks DTO exists because backend endpoint/contract is missing.

## Recommended alignment direction

1. Generate DTOs from OpenAPI and treat them as source-of-truth transport models.
2. Introduce per-feature mapper layer (`generated DTO -> UI view model`) in feature data-access facades.
3. Keep presentation components free from transport-specific shapes and enum serialization concerns.
4. Normalize dates as `Date`/`string` at mapper boundary; never in templates/components.
5. Use explicit fallback handling for nullable export fields and missing optional analytics segments.

## Priority remediation backlog

- **P0**
  - Dashboard + Analytics mapper contracts from `DashboardKpisResponse`.
  - Campaigns/Templates list model alignment with enum normalization.
  - Exports list/status alignment with nullable lifecycle fields.
- **P1**
  - Campaign/template detail CRUD models.
  - Auth claim-to-session mapping.
  - Tracking link generation model usage.
- **Blocked by backend**
  - Tracking events list read model endpoint/DTO.
  - Tasks queue list endpoint/DTO.
