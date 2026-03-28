# Frontend Auth-Aware Request Flow Plan

Last updated: 2026-03-28  
Owner: Codex  
Scope: `frontend/src/app/**` request execution path for all backend API calls (generated-client-first).

## 1) Objective

Define a deterministic, auth-aware request flow so every frontend API call:

- attaches access tokens only when required,
- handles expiration with a single refresh path,
- fails safely to logout/unauthenticated UX when refresh is not possible,
- avoids duplicated auth logic in feature code,
- stays compatible with generated OpenAPI clients and centralized facade patterns.

This plan extends and is consistent with:

- `docs/frontend-centralized-api-consumption-pattern.md`
- `docs/form-crud-endpoint-wiring-plan.md`
- `docs/dto-to-ui-mapping-strategy.md`

## 2) Canonical auth-aware request flow

## 2.1 Request path (logical diagram)

```text
UI Container/Store
  -> Feature Facade (features/*/data-access)
    -> Core API Wrapper (core/api)
      -> Auth-Aware HTTP Interceptor Chain
         1) Correlation/Trace Interceptor
         2) Auth Token Attachment Interceptor
         3) Unauthorized/Refresh Interceptor
         4) API Error Translation Interceptor
      -> Generated OpenAPI Service (core/api/generated)
        -> Gateway (/api/*)
          -> Backend API
```

## 2.2 Response/error path (logical diagram)

```text
Backend API / Gateway response
  -> Unauthorized/Refresh Interceptor (401 branch)
     -> attempt single-flight refresh
     -> replay original request on refresh success
     -> emit auth-failed event on refresh failure
  -> API Error Translation Interceptor
  -> Core API Wrapper normalized envelope
  -> Feature Facade view-state mapping
  -> UI state + fallback UX (toast/banner/redirect)
```

## 3) Interceptor responsibilities and ordering

## 3.1 Correlation/Trace interceptor

- Ensure `X-Correlation-ID` is present for every outbound API request.
- Preserve existing correlation id generated upstream where available.
- Expose correlation id in translated error metadata for support diagnostics.

## 3.2 Auth token attachment interceptor

- Attach `Authorization: Bearer <access-token>` only for protected endpoints.
- Do not attach bearer token to explicitly public endpoints (`/api/health`, auth bootstrap metadata endpoints if public).
- Read token from centralized auth session service only; never from feature-local storage logic.

## 3.3 Unauthorized/refresh interceptor

- Handle `401` responses for protected endpoints.
- Run refresh with **single-flight** concurrency control (one refresh call shared across concurrent 401s).
- Replay queued failed requests only after refresh success.
- On refresh failure/invalid session:
  - clear auth session,
  - emit global auth-expired signal,
  - navigate to unauthorized/login route with context reason,
  - stop replay and return deterministic `unauthorized` error model.

## 3.4 API error translation interceptor

- Convert transport/status errors into shared `ApiErrorModel` kinds.
- Preserve `traceId`, correlation id, status code, and field-level validation details.
- Keep feature components free from raw `HttpErrorResponse` branching.

## 4) Session lifecycle contract

## 4.1 Startup/bootstrap

- On app bootstrap, initialize auth state before first protected feature load.
- If no valid access token and no refresh path -> unauthenticated state.
- If tokens exist, perform lightweight validity check (expiration skew tolerance).

## 4.2 Refresh strategy

- Trigger refresh only on qualifying 401s from protected API calls.
- Reject refresh loops: each original request may be replayed at most once after refresh.
- Respect server clock skew with proactive renewal threshold (e.g., refresh if token close to expiry).

## 4.3 Logout strategy

- User-initiated logout must:
  - revoke/terminate session at IdP when supported,
  - clear local tokens/session snapshot,
  - cancel in-flight protected requests,
  - route to post-logout public screen.

## 4.4 Unauthenticated fallback UX

- For route guard denial (not authenticated): redirect to `/unauthorized?reason=auth`.
- For role denial: redirect to `/unauthorized?reason=role`.
- For runtime session expiry during page use:
  - show session-expired toast/banner,
  - preserve intended URL for post-login return,
  - route to auth entrypoint or unauthorized page per deployment mode.

## 5) Endpoint auth coverage checklist

All API calls must be classified in one of two groups and enforced in interceptor tests.

## 5.1 Public endpoints (no token)

| Endpoint family | Method(s) | Token attached | Notes |
|---|---|---|---|
| `/api/health` | GET | No | Must remain probe-safe/anonymous. |
| `/api/health/live` | GET | No | Liveness probe. |
| `/api/health/ready` | GET | No | Readiness probe. |

## 5.2 Protected endpoints (token required)

| Endpoint family | Method(s) | Token attached | 401 handling |
|---|---|---|---|
| `/api/access/*` | GET | Yes | Refresh once, else unauthorized fallback. |
| `/api/campaigns*` | GET/POST/PUT/DELETE | Yes | Refresh once, no silent infinite retry. |
| `/api/templates*` | GET/POST/PUT/DELETE | Yes | Refresh once, map validation/conflict errors. |
| `/api/tracking*` | GET/POST | Yes | Rate-limit aware + auth-aware handling. |
| `/api/tasks*` | GET/POST/PUT | Yes | Refresh once, preserve request correlation metadata. |
| `/api/analytics*` | GET | Yes | Refresh once, maintain dashboard cancellation semantics. |
| `/api/exports*` | GET/POST | Yes | Refresh once, preserve polling behavior after replay. |

> Note: All frontend traffic must target gateway-routed paths only (no direct backend origin calls).

## 6) Required implementation work packages (execution-ready)

1. Add `core/auth/auth-session.service.ts` contract for token read/write/clear/refresh orchestration.
2. Add interceptor chain under `core/api/interceptors/` with deterministic ordering and unit tests.
3. Add `core/auth/auth-events.service.ts` for session-expired and logout broadcast.
4. Wire auth-failure navigation integration in routing shell (single source of redirect truth).
5. Add integration tests (HttpClientTestingModule) for:
   - public request without token,
   - protected request with token,
   - concurrent 401 -> single refresh,
   - refresh fail -> logout + unauthorized redirect,
   - replay-once guarantee.

## 7) Definition of done

This subdomain is considered complete when all are true:

- Every generated-client call traverses auth-aware interceptors.
- Public endpoint allowlist is enforced and tested.
- Protected endpoint list requires token and 401-refresh handling.
- Refresh flow is single-flight and replay-once.
- Expired/invalid session consistently transitions to unauthenticated fallback UX.
- No feature module contains direct token-attachment or refresh logic.
