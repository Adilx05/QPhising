# Identity Role Claim Mapping (Domain Source of Truth)

This document defines the canonical role model and claim mapping rules for identity resolution.

## Roles

- `Admin`
- `Operator`
- `Viewer`

## Accepted claim mappings

The Identity domain accepts role assertions from these claim paths:

- `role`
- `roles`
- `realm_access.roles`
- `resource_access.qphising-api.roles`

Each role value must match one of the canonical role names above.

## Notes

- Domain model source: `Domain/Identity/*`
- Application and API layers must consume these role definitions instead of redefining role names.
- Role matching is case-insensitive at the domain boundary.
