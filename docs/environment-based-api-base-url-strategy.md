# Environment-Based API Base URL Strategy

Last updated: 2026-03-28  
Owner: Codex  
Scope: `frontend/**`, `docker-compose.yml`, gateway-routed API consumption

## Objective

Define one production-safe, environment-aware strategy for frontend API base URL resolution so all browser API traffic targets the gateway entrypoint without hardcoded per-feature URLs.

## Authoritative configuration contract

Frontend runtime configuration must expose the following keys through a single centralized config service:

| Key | Required | Type | Default/Fallback | Notes |
|---|---|---|---|---|
| `gatewayBaseUrl` | Yes | string (absolute URL) | none (startup fail) | Canonical browser-visible gateway origin (e.g., `http://localhost:5001`). |
| `apiPathPrefix` | Yes | string (path) | `/api` | Must begin with `/`; normalized without trailing slash. |
| `apiBaseUrl` | Optional (derived) | string (absolute URL) | Derived from `gatewayBaseUrl + apiPathPrefix` | If supplied, must match derived normalized value, otherwise startup fail. |
| `requestTimeoutMs` | No | number | `15000` | Global API request timeout for generated client wrappers/facades. |

### Derived value rule

`apiBaseUrl` authoritative runtime value:

1. If `apiBaseUrl` is provided, normalize and validate it.
2. Compute `derivedApiBaseUrl = normalizeUrl(gatewayBaseUrl) + normalizePath(apiPathPrefix)`.
3. If both exist and differ after normalization, fail startup with explicit config error.
4. If `apiBaseUrl` is absent, use `derivedApiBaseUrl`.

## Environment matrix

| Environment | Frontend config source | `gatewayBaseUrl` expected pattern | `apiPathPrefix` | Notes |
|---|---|---|---|---|
| Development (local browser + ng serve) | `frontend/src/environments/environment.ts` (or local runtime config override) | `http://localhost:<gateway-port>` | `/api` | Must be browser-reachable from host machine. |
| Staging (containerized immutable image) | Runtime-injected `/assets/config.json` | `https://<staging-domain>` | `/api` | Deployment injects config per environment without rebuilding image. |
| Production (containerized immutable image) | Runtime-injected `/assets/config.json` | `https://<production-domain>` | `/api` | Must be release-gated by startup validation and smoke checks. |

## Startup validation contract

Validation is mandatory during app bootstrap (`APP_INITIALIZER` or equivalent pre-bootstrap path):

1. Required keys exist (`gatewayBaseUrl`, `apiPathPrefix`).
2. `gatewayBaseUrl` parses as absolute HTTP(S) URL.
3. `apiPathPrefix` starts with `/` and contains no query/hash.
4. `apiBaseUrl` (if present) parses as absolute HTTP(S) URL.
5. Derived/explicit `apiBaseUrl` consistency check passes.
6. Final `apiBaseUrl` host equals `gatewayBaseUrl` host unless an explicit, approved exception is configured.

If any check fails:
- fail app startup,
- log deterministic config error code (`CFG_BASEURL_*`),
- render user-safe fatal configuration screen (no silent fallback to hardcoded URLs).

## Hardcoding prohibition and layering rule

- Feature components/services must never define endpoint origins or handcrafted root URLs.
- Generated OpenAPI clients/facades must receive base URL only from centralized runtime config provider.
- Gateway is the single frontend API entrypoint; direct browser calls to internal backend services are disallowed.

## Implementation-ready acceptance criteria

1. A single config provider supplies normalized `gatewayBaseUrl` and effective `apiBaseUrl` to all generated client wrappers.
2. Environment sources are explicitly defined for dev (`environment.ts`) and staging/prod (runtime-injected `assets/config.json`).
3. Startup validation fails fast on missing/invalid/mismatched URL config.
4. No hardcoded API origin literals exist in `frontend/src/app/**`.
5. Gateway-only routing policy is documented and enforced by configuration defaults.

## Verification commands

```bash
rg -n "gatewayBaseUrl|apiBaseUrl|apiPathPrefix|requestTimeoutMs" docs/environment-based-api-base-url-strategy.md
rg -n "Define environment-based API base URL strategy|environment-based-api-base-url-strategy" TASKS.md docs/environment-based-api-base-url-strategy.md
```
