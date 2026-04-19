# Tracking API Contracts

## Public Endpoints

- `GET /p/{slug}`: resolve published tracking landing-page metadata.
- `POST /api/tracking/pages/{trackingPageId}/visits`: ingest visit events with deduplication and privacy policy handling.

## Authenticated Management Endpoints

- `GET /api/tracking/pages`: list tracking pages (search/sort/paging on client side).
- `GET /api/tracking/pages/{trackingPageId}`: read tracking page detail.
- `POST /api/tracking/pages`: create tracking page.
- `PUT /api/tracking/pages/{trackingPageId}`: update tracking page.
- `POST /api/tracking/pages/{trackingPageId}/publish`: publish lifecycle transition.
- `POST /api/tracking/pages/{trackingPageId}/archive`: archive lifecycle transition.
- `DELETE /api/tracking/pages/{trackingPageId}`: delete tracking page.

## Analytics Endpoints

- `GET /api/tracking/pages/{trackingPageId}/analytics`: page-level summary/trends/recent events.
- `GET /api/tracking/analytics/overview`: cross-page overview metrics, trends, top pages, and recent stream.

## Contract Governance

- Swagger/OpenAPI is the source for generated frontend proxies.
- Proxy regeneration and determinism checks are required whenever API contracts change.
- Error responses use ProblemDetails-compatible semantics for validation, conflict, not-found, and authorization failures.
