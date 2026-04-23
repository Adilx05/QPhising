# Production Readiness Review (Phase 9)

## Security

- Admin/management tracking routes are authenticated and policy-protected.
- Public tracking ingress routes are rate-limited.
- Visit payload constraints and privacy hashing controls are enforced.
- Security audit middleware captures key auth/rate-limit denial events.

## Observability

- API and Gateway emit structured JSON logs.
- Correlation ids (`X-Correlation-Id`) are propagated across services.
- CI contains quality gates for Swagger, proxy consistency, and gateway alignment.

## Reliability

- EF Core migrations are configured and tracked.
- Runtime supports local compose-based API/Gateway/PostgreSQL (+ optional Redis).
- Setup/runtime configuration templates exist for local/staging/production.
- Health endpoints are split by liveness/readiness with a shared response contract on API and Gateway:
  - `GET /health/live`: process liveness only; expected `Healthy` when service is running.
  - `GET /health/ready`: dependency readiness; expected `Healthy` in steady state, `Degraded` when optional dependencies (e.g., Redis) are unavailable/disabled, `Unhealthy` when critical dependencies fail.

## Remaining Release Gate

Before production cut, run full backend/frontend build and test suites in CI and ensure no environment-level warnings remain.
