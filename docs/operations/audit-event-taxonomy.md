# Audit Event Taxonomy

This document defines the canonical taxonomy for security/action audit events captured into the `audit_log_entries` read model.

## Event Fields

Every stored event contains:

- `timestampUtc`
- `actor`
- `action`
- `resource`
- `outcome`
- `outcomeCode`
- `correlationId`
- `ipHash`

## Critical Event Actions

| Action | Trigger | Outcome semantics |
|---|---|---|
| `security.unauthorized` | HTTP `401` responses | `unauthorized` |
| `security.forbidden` | HTTP `403` responses | `forbidden` |
| `security.rate_limited` | HTTP `429` responses | `throttled` |
| `campaign.delete` | `DELETE /api/campaigns/{campaignId}` | `success` for 2xx, `failure` otherwise |
| `campaign.start` | `POST /api/campaigns/{campaignId}/start` | `success` for 2xx, `failure` otherwise |
| `campaign.pause` | `POST /api/campaigns/{campaignId}/pause` | `success` for 2xx, `failure` otherwise |
| `campaign.complete` | `POST /api/campaigns/{campaignId}/complete` | `success` for 2xx, `failure` otherwise |
| `campaign.cancel` | `POST /api/campaigns/{campaignId}/cancel` | `success` for 2xx, `failure` otherwise |
| `template.save` | `POST /api/templates` and `PUT /api/templates/{templateId}` | `success` for 2xx, `failure` otherwise |
| `template.delete` | `DELETE /api/templates/{templateId}` | `success` for 2xx, `failure` otherwise |
| `template.publish` | `POST /api/templates/{templateId}/publish` | `success` for 2xx, `failure` otherwise |
| `tracking.publish` | `POST /api/tracking/pages/{trackingPageId}/publish` | `success` for 2xx, `failure` otherwise |
| `tracking.archive` | `POST /api/tracking/pages/{trackingPageId}/archive` | `success` for 2xx, `failure` otherwise |
| `tracking.delete` | `DELETE /api/tracking/pages/{trackingPageId}` | `success` for 2xx, `failure` otherwise |

## Query Contract

`GET /api/audit/logs` supports:

- date range (`fromUtc`, `toUtc`)
- actor/user filter (`actor`)
- endpoint/resource filter (`endpoint`)
- status/result code filter (`outcomeCode`)
- correlation search (`correlationId`)
- pagination (`page`, `pageSize`)
- sorting (`sortBy`, `sortDirection`)

## Operational Notes

- IP is persisted only as a SHA-256 hash in audit records.
- Correlation id comes from `X-Correlation-Id` middleware propagation.
- Taxonomy changes must update this document and corresponding UI labels/filters.
