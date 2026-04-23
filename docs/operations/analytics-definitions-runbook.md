# Analytics Definitions & Runbook

## Metric Definitions

- **Total Visits**: count of all visit events in the selected interval and filter scope.
- **Unique Visitors**: distinct visitor key count; session id is used when available, otherwise visitor fingerprint.
- **Top Pages**: pages ranked by total visits, then unique visitors, then slug for deterministic ordering.
- **Recent Visits**: newest visit events ordered by occurrence timestamp descending.
- **Trend Buckets**: grouped visits by selected window (hour/day/week or fixed minute buckets for page-level analytics).

## Edge-case Handling

- **Bot Filtering**: when enabled and requested, bot-like user-agent signatures are excluded.
- **Duplicate Suppression**: ingestion checks for same page/session/fingerprint within deduplication window.
- **Timezone Boundaries**: overview trend windows align using requested timezone offset and then normalize to UTC.

## Operational Checks

1. Validate API + Gateway startup and health checks.
   - API: `GET /health/live` (process), `GET /health/ready` (PostgreSQL + Redis optional + Keycloak metadata/token probe).
   - Gateway: `GET /health/live` (process), `GET /health/ready` (downstream API readiness + Keycloak metadata/token probe).
   - Expected readiness payload contract on both services:
     - `overallStatus`: `Healthy | Degraded | Unhealthy`
     - `latencyMs`: total readiness probe latency
     - `dependencies[]`: each dependency item includes `name`, `status`, `latencyMs`, `failureReason`
2. Validate tracking CRUD flow and publish lifecycle transitions.
3. Validate public slug resolution and visit ingestion with representative traffic.
4. Validate analytics overview and page detail endpoint outputs for known fixture datasets.
5. Run proxy-sync and gateway/swagger consistency scripts before release.

## Incident Triage Tips

- If analytics totals are unexpectedly low: confirm bot-exclusion flags and date filters.
- If unique visitor counts spike: verify session id quality and fallback fingerprint generation inputs.
- If ingestion throughput regresses: inspect duplicate-guard query timing and database resource saturation.
- If readiness is `Degraded`: inspect optional dependency failures (typically Redis) before escalating to critical incident.
- If readiness is `Unhealthy`: inspect PostgreSQL and Keycloak probes first, then downstream API probe from Gateway.
