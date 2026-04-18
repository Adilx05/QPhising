# API Resource Naming and Version Scope Conventions

This document defines Domain-owned conventions for naming API resources and scoping route versions.

## Resource naming convention

All top-level API resource route segments must:

- be lowercase
- use kebab-case
- be plural nouns
- avoid transport verbs in the segment name

Examples:

- `setup-configurations`
- `runtime-configurations`
- `proxy-validations`

Non-examples:

- `SetupConfiguration`
- `setup_config`
- `test-db`

## Version scope convention

A resource is modeled as one of these scopes:

- `Unversioned`: stable operational/setup endpoints that are not customer-facing public contracts yet.
- `Versioned`: public API resources that must include an explicit route segment such as `v1`.

When a resource is versioned, its version segment must be explicitly declared in the domain convention model.

## Purpose in the architecture

- Domain defines the naming/version invariants.
- Application and API layers consume these conventions when shaping operation IDs, route templates, and Swagger metadata.
- Swagger quality gates should fail if published endpoints violate these conventions.
