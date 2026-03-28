# Form CRUD endpoint wiring plan

## Scope and intent

This document defines how frontend form workflows must wire create/update/delete actions to backend CQRS command endpoints through the generated OpenAPI Angular client (never handwritten `HttpClient` transport in feature components).

The plan covers currently implemented modules and explicitly records contract gaps where a required command endpoint does not yet exist.

## Architecture and layering constraints

- Feature containers/components call **feature facade** APIs only.
- Facades call generated OpenAPI services under `core/api/generated`.
- Facades map generated DTOs into feature UI models via `core/api/mappers`.
- Validation and error normalization flows through shared API error mapping in `core/api/errors`.
- Controllers remain transport-only in backend; command/query business logic remains in application handlers.

Canonical flow:

1. Form component emits `submit` / `delete` intent.
2. Feature facade shapes request payload DTO for generated client method.
3. Generated client executes gateway-routed API request.
4. Validation/problem response is normalized to a typed `FormErrorState` model.
5. On success, facade refreshes detail/list state using read endpoints (backend truth source).

## Shared form contract

### Request lifecycle states

Each form-capable feature must expose:

- `idle`
- `submitting`
- `submitSucceeded`
- `submitFailedValidation`
- `submitFailedRecoverable`
- `submitFailedFatal`

### Validation error projection

When API returns `application/problem+json` with `HttpValidationProblemDetails`:

- Project `errors[fieldName]` to field-level messages.
- Project `title`/`detail` to form-level banner.
- Preserve `traceId` for diagnostics footer/telemetry.

When API returns domain/business 400 ProblemDetails without field dictionary:

- Project `detail` to form-level error summary.
- Keep existing user input unchanged.

### Update synchronization strategy

- **Create/Update/Delete command completion is pessimistic.**
  - Wait for command success before mutating canonical list/detail stores.
- **UI responsiveness may be optimistic for local affordances only** (disable buttons, temporary spinner rows), but data truth must be reloaded from backend after success.
- After successful command:
  - refresh detail view for edited entity;
  - refresh active list page/query to ensure server ordering/filter invariants.

## Module wiring matrix

## 1) Campaigns

### Create campaign form

- Endpoint: `POST /api/v1/campaigns`
- Backend request DTO:
  - `name`
  - `templateType`
  - `htmlContent`
  - `startDate`
  - `endDate`
- Generated-client action: `CampaignsService.create(...)` (name based on generated output conventions).
- Success handling:
  - expect `201 Created` + created entity contract.
  - navigate to campaign detail route and refresh detail query.
- Validation projection:
  - enforce date range and required fields from problem payload.

### Update campaign form

- Endpoint: `PUT /api/v1/campaigns/{campaignId}`
- Request payload: same shape as create (excluding id in body).
- Generated-client action: `CampaignsService.update(campaignId, payload)`.
- Success handling:
  - expect `200 OK`.
  - refresh detail and current campaigns list.

### Delete campaign form/action

- **Gap:** no campaign delete command endpoint currently exposed.
- Required backend addition (future subtask): `DELETE /api/v1/campaigns/{campaignId}` with CQRS command handler.
- Frontend behavior until available:
  - hide destructive delete action,
  - show non-blocking “Not supported yet” state only in operator/admin contexts where action would exist.

## 2) Templates

### Create template form

- Endpoint: `POST /api/v1/templates`
- Request DTO:
  - `name`
  - `type`
  - `htmlContent`
  - `variables` (optional)
- Generated-client action: `TemplatesService.create(...)`.
- Success handling:
  - expect `201 Created`.
  - refresh templates list and route to template detail/editor.

### Update template form

- Endpoint: `PUT /api/v1/templates/{templateId}`
- Request DTO: same as create.
- Generated-client action: `TemplatesService.update(templateId, payload)`.
- Success handling: refresh detail + list.

### Delete template form/action

- **Gap:** no hard-delete endpoint; lifecycle uses archive command.
- Available replacement command endpoint:
  - `POST /api/v1/templates/{templateId}/archive`
- Wiring rule:
  - UI label: “Archive template” (not “Delete”).
  - treat as destructive confirmation workflow.
  - on success, refresh templates list and close detail/editor if archived item is open.

## 3) Exports

### Create export job form

- Endpoint: `POST /api/v1/exports`
- Request DTO:
  - `exportType`
  - `format`
  - `correlationId` (optional)
- Generated-client action: `ExportsService.queue(...)`.
- Success handling:
  - expect `201 Created` with job contract.
  - append/refresh jobs list and start status polling flow via `GET /api/v1/exports/{exportJobId}`.

### Update export form

- **Not applicable:** export jobs are immutable command submissions.

### Delete export form/action

- **Gap:** no cancel/delete command endpoint currently exposed.
- Required backend addition (future subtask): cancel/delete command depending on domain rule.
- Frontend behavior until available:
  - no delete/cancel button in live UI,
  - expose status-only workflow.

## 4) Tracking link generator

### Create tracking link form

- Endpoint: `POST /api/v1/tracking/links`
- Request DTO:
  - `campaignId`
  - `recipientEmail`
- Generated-client action: `TrackingService.generateTrackingLink(...)`.
- Success handling:
  - expect `201 Created` with generated link payload.
  - render copy-to-clipboard workflow and audit event.

### Update/Delete tracking records

- **Not applicable with current contracts:** tracking records are event-driven and not user-editable through form CRUD.

## 5) Tasks module

- **Gap:** no task create/update/delete command endpoints currently exposed in API controllers.
- Required backend additions:
  - task creation command endpoint,
  - task status/update command endpoint,
  - optional task cancellation/delete endpoint.
- Frontend behavior until available:
  - keep list/read-only UX for queue visibility.

## Authorization and role constraints for form actions

- Form command actions must be shown/enabled only when current claims satisfy endpoint policy.
- From existing API policies:
  - campaign/template create-update/archive/schedule/activate require `Operator` (or `Admin` via policy hierarchy),
  - export job queue requires `Viewer`+,
  - tracking link creation requires `Operator`+.
- Facades must handle `403` by mapping to an authorization error state and triggering UX-level access messaging.

## Retry and idempotency guidance

- Create/update/archive/schedule/activate actions should provide explicit manual retry button after recoverable network failures.
- Avoid automatic retries for non-idempotent create commands unless a request correlation mechanism is present.
- Use `correlationId` when queueing export jobs to support dedup-friendly behavior.

## Implementation checklist (execution follow-up)

1. Generate fresh OpenAPI client and confirm command operations exist.
2. Add/extend per-feature facade methods wrapping generated command operations.
3. Implement shared form error mapper for ProblemDetails + validation dictionary.
4. Integrate create/update/archive forms in campaigns/templates features.
5. Integrate create export and tracking-link forms.
6. Explicitly remove/avoid any handwritten duplicate transport calls.
7. Add integration tests for facade error mapping and success refresh behavior.
