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
| Setup | `Domain/Setup` | API OpenAPI (`/api/setup/*`) | Generated setup proxy |
| Runtime Configuration | `Domain/RuntimeConfiguration` | API OpenAPI (`/api/configuration/*`) | Generated configuration proxy |
| Campaign | `Domain/Campaigns` | API OpenAPI (`/api/campaigns/*`) | Generated campaign proxy |
| Template | `Domain/Templates` | API OpenAPI (`/api/templates/*`) | Generated template proxy |
| Tracking | `Domain/Tracking` | API OpenAPI (`/api/tracking/*`) | Generated tracking proxy |
| Background Jobs | `Domain/Jobs` | API OpenAPI (`/api/jobs/*`) | Generated jobs proxy |
| Analytics | `Domain/Analytics` | API OpenAPI (`/api/analytics/*`) | Generated analytics proxy |
| Export | `Domain/Exports` | API OpenAPI (`/api/exports/*`) | Generated export proxy |
| Identity & Access | `Domain/Identity` | API OpenAPI (`/api/auth/*`) | Generated auth proxy |
| Gateway Mapping | `Domain/Gateway` | Gateway/API route contracts | Gateway routing config |

## Invariants

1. Contract-breaking API changes require synchronized Application contract updates and proxy regeneration.
2. Domain invariants are enforced in domain and application layers only, never in API transport DTOs.
3. Application layer maps domain models to contracts; API layer maps contracts to HTTP.
4. Contract drift is a build-time validation concern and must fail CI when detected.
