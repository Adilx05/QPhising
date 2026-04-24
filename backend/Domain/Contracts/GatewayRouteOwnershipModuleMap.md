# Gateway Route Ownership Module Map

This specification defines domain ownership for gateway upstream route templates and the intended authentication posture for each route group.

## Ownership Rules

- Every upstream route template must map to exactly one owning gateway module.
- Public landing and public visit-ingestion routes are anonymously reachable by design.
- Catch-all API routes are authenticated and represent protected runtime module traffic.
- Health routes stay unauthenticated for infrastructure probes and deployment automation.

## Current Module Map Baseline

| Upstream Path Template | Owning Module | Requires Authentication | Purpose |
| --- | --- | --- | --- |
| `/p/{slug}` | `GatewayModule.PublicTracking` | No | Public campaign/tracking landing resolution route. |
| `/api/tracking/pages/{slug}/visits` | `GatewayModule.PublicTracking` | No | Public visit-ingestion endpoint for slug-based tracking pages. |
| `/api/{everything}` | `GatewayModule.PlatformApi` | Yes | Authenticated platform APIs routed through gateway. |
| `/health/live` | `GatewayModule.Health` | No | Liveness checks for gateway/application monitoring. |
| `/health/ready` | `GatewayModule.Health` | No | Readiness checks for dependency-aware monitoring and rollouts. |

## Invariants

1. Duplicate upstream path templates are invalid within a single ownership map.
2. Ownership definitions are domain-managed and framework-agnostic.
3. Infrastructure routing config (Ocelot JSON and host-mapped health endpoints) must remain aligned with this map.
