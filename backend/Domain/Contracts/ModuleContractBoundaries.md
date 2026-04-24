# Module Contract-Source Boundaries

This document defines module-level contract ownership boundaries for backend domain models and transport contracts.

## Boundary Rules

- Domain models are owned by `backend/Domain` and must remain framework-agnostic.
- Application request/response contracts are owned by `backend/Application/Contracts`.
- API transport contracts and OpenAPI metadata are owned by `backend/API`.
- Generated frontend proxies are owned by the proxy-generation workflow and must derive from API OpenAPI output.
- No module may expose its domain entities directly as API transport contracts.

## Module Ownership Map

| Module | Domain Model Owner | Contract Source of Truth | Exposed Via |
| --- | --- | --- | --- |
| Health | `Domain/Health` | API OpenAPI (`/health/live`, `/health/ready`, `/api/health`) | Operational health payloads and dashboard status reads |
| Campaign | `Domain/Campaigns` | API OpenAPI (`/api/campaigns/*`) | Generated campaign proxy |
| Template | `Domain/Templates` | API OpenAPI (`/api/templates/*`) | Generated template proxy |
| Tracking | `Domain/Tracking` | API OpenAPI (`/api/tracking/*`, `/p/{slug}`) | Generated tracking proxy + public landing route |
| Analytics | `Domain/Analytics` | API OpenAPI (`/api/tracking/analytics/*`) | Generated analytics proxy |
| Audit | `Domain/Audit` | API OpenAPI (`/api/audit/*`) | Generated audit proxy |
| Gateway Mapping | `Domain/Gateway` | API OpenAPI (`/api/gateway/route-policies`) + gateway route contracts | Generated gateway proxy + Ocelot routing config |
| Identity & Access | `Domain/Identity` | API OpenAPI (`/api/auth/*`) | Generated auth proxy |
| Proxy Validation | `Domain/Contracts` | API OpenAPI (`/api/proxy-validation/*`) | Generated proxy validation surface |

## Invariants

1. Contract-breaking API changes require synchronized Application contract updates and proxy regeneration.
2. Domain invariants are enforced in domain and application layers only, never in API transport DTOs.
3. Application layer maps domain models to contracts; API layer maps contracts to HTTP.
4. Contract drift is a build-time validation concern and must fail CI when detected.
