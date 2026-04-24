# Tracking API Contracts

> Scope note: This document remains at `api-contracts-tracking.md` for backward compatibility, but now tracks cross-module contracts (Tracking, Campaign, Template, Audit, Reports) that directly affect tracking workflows. It can be renamed to `api-contracts.md` in a later docs consolidation pass.

## Public Endpoints

- `GET /p/{slug}`: resolve published tracking landing-page metadata.
- `POST /api/tracking/pages/{trackingPageId}/visits`: ingest visit events with deduplication and privacy policy handling.
- `POST /api/tracking/pages/{slug}/visits`: ingest visit events through public slug resolution (campaign-aware).

Role baseline: **Anonymous** (no Viewer/Operator/Admin role required).

## Tracking Management Endpoints

- `GET /api/tracking/pages`: list tracking pages (search/sort/paging on client side).
- `GET /api/tracking/pages/{trackingPageId}`: read tracking page detail.
- `POST /api/tracking/pages`: create tracking page.
- `PUT /api/tracking/pages/{trackingPageId}`: update tracking page.
- `POST /api/tracking/pages/{trackingPageId}/publish`: publish lifecycle transition.
- `POST /api/tracking/pages/{trackingPageId}/archive`: archive lifecycle transition.
- `DELETE /api/tracking/pages/{trackingPageId}`: delete tracking page.

Role baseline: **Viewer** for read endpoints, **Operator** for create/update/publish/archive, **Admin** for delete.

## Tracking Analytics Endpoints

- `GET /api/tracking/pages/{trackingPageId}/analytics`: page-level summary/trends/recent events.
- `GET /api/tracking/analytics/overview`: cross-page overview metrics, trends, top pages, and recent stream.

Role baseline: **Viewer**.

## Campaign Endpoints

Base route: `/api/campaigns`

- `GET /api/campaigns`
- `GET /api/campaigns/{campaignId}`
- `POST /api/campaigns`
- `PUT /api/campaigns/{campaignId}`
- `POST /api/campaigns/{campaignId}/schedule`
- `POST /api/campaigns/{campaignId}/start`
- `POST /api/campaigns/{campaignId}/pause`
- `POST /api/campaigns/{campaignId}/complete`
- `POST /api/campaigns/{campaignId}/cancel`
- `DELETE /api/campaigns/{campaignId}`

Role baseline: **Viewer** for read endpoints, **Operator** for create/update/schedule/start/pause/complete, **Admin** for cancel/delete.

## Template Endpoints

Base route: `/api/templates`

- `GET /api/templates`
- `GET /api/templates/{templateId}`
- `POST /api/templates`
- `PUT /api/templates/{templateId}`
- `POST /api/templates/{templateId}/publish`
- `POST /api/templates/{templateId}/archive`
- `DELETE /api/templates/{templateId}`

Role baseline: **Viewer** for read endpoints, **Operator** for create/update/publish/archive, **Admin** for delete.

## Audit Endpoints

- `GET /api/audit/logs`

Role baseline: **Operator**.

## Reports Endpoints

- `GET /api/tracking/analytics/reports/export`

Role baseline: **Viewer**.

## Contract Governance

- Swagger/OpenAPI is the source for generated frontend proxies.
- Proxy regeneration and determinism checks are required whenever API contracts change.
- Error responses use ProblemDetails-compatible semantics for validation, conflict, not-found, and authorization failures.
