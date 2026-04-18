# Gateway Route Ownership Module Map

This specification defines domain ownership for gateway upstream route templates and the intended authentication posture for each route group.

## Ownership Rules

- Every upstream route template must map to exactly one owning gateway module.
- Setup bootstrap routes remain publicly reachable until setup completion flow grants normal app access.
- Catch-all API routes are authenticated and represent runtime module traffic.
- Health routes stay unauthenticated for infrastructure probes.

## Current Module Map Baseline

| Upstream Path Template | Owning Module | Requires Authentication | Purpose |
| --- | --- | --- | --- |
| `/api/setup/{everything}` | `GatewayModule.Setup` | No | Setup wizard path while runtime bootstrap is incomplete. |
| `/api/{everything}` | `GatewayModule.PlatformApi` | Yes | Authenticated runtime APIs routed through gateway. |
| `/health/live` | `GatewayModule.Health` | No | Liveness checks for gateway/application monitoring. |

## Invariants

1. Duplicate upstream path templates are invalid within a single ownership map.
2. Ownership definitions are domain-managed and framework-agnostic.
3. Infrastructure routing config (Ocelot JSON) must remain aligned with this map.
