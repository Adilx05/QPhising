# DTO-to-UI Mapping Strategy

Last updated: 2026-03-28  
Owner: Codex  
Scope: Frontend API integration architecture (`frontend/src/app/**`) using generated OpenAPI clients as transport source-of-truth.

## 1) Purpose

Define a strict mapping strategy that separates:

- **Generated DTOs (transport models)** from OpenAPI,
- **Feature UI models (presentation/state models)** used by containers/components/store,
- **Mapper responsibilities** (normalization, null-safe defaults, formatting-ready fields).

This strategy is mandatory for all feature modules to preserve clean architecture boundaries and avoid transport leakage into presentation code.

## 2) Architecture boundary rules

### 2.1 Layer ownership

- **Generated DTOs** live under generated-client output and are only consumed in facades/data-access code.
- **Feature mappers** own DTO-to-UI transformation and UI-to-request shaping.
- **Containers/store/components** consume only UI models (never generated DTOs directly).

### 2.2 Prohibited patterns

- Importing generated DTO types in component templates, component classes, or shared dumb components.
- Formatting API enum/date fields directly in templates.
- Passing raw generated DTO objects through app-wide state.

### 2.3 Required flow

`Component/Container -> Feature Facade -> Generated Service -> Mapper -> UI Model -> Component/Store`

For command forms:

`Form Model -> Mapper (UI-to-request) -> Generated Command Request DTO -> Generated Service`

## 3) Mapper placement and naming conventions

## 3.1 Folder structure

Per feature module, place mapper files under:

- `frontend/src/app/features/<feature>/data-access/mappers/`

Cross-feature shared primitives may live under:

- `frontend/src/app/core/api/mappers/`

## 3.2 File naming

Use deterministic file names:

- `<feature>-dto-ui.mapper.ts` (feature aggregate mapper)
- `<entity>-dto-ui.mapper.ts` (entity-focused mapper)
- `<feature>-request.mapper.ts` (UI/form -> command request)

Examples:

- `dashboard-dto-ui.mapper.ts`
- `campaign-dto-ui.mapper.ts`
- `campaign-request.mapper.ts`

## 3.3 Symbol naming

Prefer named pure functions over stateful classes.

Patterns:

- `map<SourceDtoName>To<UiModelName>(dto)`
- `map<SourceDtoName>ListTo<UiModelName>List(items)`
- `map<FormModelName>To<CreateOrUpdateRequestName>(form)`

For optional DTOs:

- `mapOptional<SourceDtoName>To<UiModelName>(dto | null | undefined)`

## 4) UI model conventions

## 4.1 UI model location

Per feature:

- `frontend/src/app/features/<feature>/models/`

Use explicit UI model names:

- `<Entity>UiModel`
- `<Feature>ViewState`
- `<Feature>Filters`

## 4.2 UI model design rules

- UI models may contain display-oriented fields (`statusLabel`, `dateDisplay`, `trendDeltaDisplay`) when required by rendering.
- UI models must avoid direct dependency on generated DTO enums where a UI-specific union/string is clearer.
- Keep optionality explicit; avoid ambiguous `any` or deeply nested nullable DTO mirrors.

## 5) Date/time mapping standards

## 5.1 Source assumption

Backend timestamps are ISO-8601 (`DateTimeOffset`) strings in generated DTOs.

## 5.2 Mapper standard

- Parse transport timestamps in mapper layer only.
- Expose one of these in UI model (chosen per feature and documented):
  - `Date` objects for logic-heavy screens,
  - precomputed safe display strings for read-only tables,
  - both raw and display fields when filtering + rendering both require them.

## 5.3 Null-safe date policy

- Missing/invalid dates must map to deterministic fallbacks:
  - nullable UI date fields (`Date | null`) for logic,
  - display fallback such as `"—"` for read-only text fields.
- Never allow `Invalid Date` to reach templates.

## 6) Enum mapping standards

## 6.1 Transport-to-UI enum normalization

- Map generated enum values to UI-owned union values/labels in mapper layer.
- Centralize per-feature enum dictionaries to avoid duplicated switch statements in components.

## 6.2 Unknown enum handling

- Unknown/forward-compatible enum values must map to a safe UI fallback:
  - canonical value: `'unknown'`,
  - label: `'Unknown'`,
  - style token: neutral severity.

## 7) Null-safe defaults and defensive mapping

## 7.1 Collections

- Null/undefined collections from DTOs map to empty arrays.

## 7.2 Strings

- Critical identity/display strings:
  - preserve real empty string if semantically meaningful,
  - otherwise map nullish values to `"—"` for display-only fields.

## 7.3 Numbers

- Numeric KPI/metrics:
  - map nullish source to explicit numeric fallback (`0`) only when domain semantics permit.
  - where unknown is semantically distinct, keep nullable (`number | null`) and provide display fallback field.

## 7.4 Nested objects

- For nullable nested objects, flatten into UI-safe optional segments or apply default object factories.
- Avoid optional chaining in templates for transport fields; resolve in mappers first.

## 8) Error-resilient mapper contract

- Mapper functions must be pure and side-effect free.
- Mapper functions must not perform HTTP, store updates, or toast notifications.
- Mapper failures should be treated as programming errors (typed guards + deterministic fallback), not silently swallowed in components.

## 9) Per-feature mapper ownership matrix

| Feature | Primary transport DTO families | UI model families | Mapper owner path | Required mapper set |
|---|---|---|---|---|
| Dashboard | `DashboardKpisResponse`, `AnalyticsTrendPoint`, campaign list DTOs | `DashboardKpi`, `DashboardTrendPoint`, `DashboardCampaignRow` | `frontend/src/app/features/dashboard/data-access/mappers/` | KPI mapper, trend mapper, campaign-row mapper |
| Campaigns | `ListCampaignsResponse`, `CampaignListItemResponse`, detail/create/update DTOs | `CampaignListRowUiModel`, `CampaignDetailUiModel`, `CampaignFormModel` | `frontend/src/app/features/campaigns/data-access/mappers/` | list mapper, detail mapper, request mapper |
| Templates | list/detail/create/update template DTOs | `TemplateListRowUiModel`, `TemplateDetailUiModel`, `TemplateFormModel` | `frontend/src/app/features/templates/data-access/mappers/` | list mapper, detail mapper, request mapper |
| Tracking | link-generation + click-processing DTOs (and future events list DTOs) | `TrackingLinkUiModel`, `TrackingEventRowUiModel` | `frontend/src/app/features/tracking/data-access/mappers/` | generate-link mapper, events mapper |
| Tasks | future task queue/list DTOs | `TaskRowUiModel`, `TaskDetailUiModel` | `frontend/src/app/features/tasks/data-access/mappers/` | list/detail mapper |
| Analytics | analytics summary/trend DTOs | `AnalyticsSummaryUiModel`, `AnalyticsTrendUiModel` | `frontend/src/app/features/analytics/data-access/mappers/` | summary mapper, trend mapper |
| Exports | `ExportJobContract` + queue/status/download DTOs | `ExportJobRowUiModel`, `ExportJobDetailUiModel`, `ExportRequestUiModel` | `frontend/src/app/features/exports/data-access/mappers/` | list mapper, status mapper, request mapper |
| Auth/Core | access probe + token claim DTOs | `SessionState`, `ResolvedRolesUiModel` | `frontend/src/app/core/auth/mappers/` | claims/session mapper, role mapper |

## 10) Implementation checklist (definition of done)

A feature is compliant only when all are true:

1. Generated DTO imports are limited to facade/data-access and mapper files.
2. Components/containers use only UI models from feature `models/`.
3. Date/enum normalization occurs in mapper layer, not templates.
4. Null-safe defaults are explicit and unit-testable.
5. Form submission uses request mapper (`FormModel -> Request DTO`) with no ad-hoc payload assembly in components.
6. New endpoint integrations add or update mapper tests alongside facade usage.

## 11) Governance rules for future work

- Any new frontend API integration PR must include mapper additions/updates before component wiring.
- PR reviews must reject direct generated DTO usage outside data-access/facade/mapper boundary.
- If backend contract changes, regenerate clients first, then update mappers and UI models (never patch around mismatch in components).
