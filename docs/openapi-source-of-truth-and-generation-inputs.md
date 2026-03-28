# OpenAPI Source of Truth and Generation Inputs

- Date: 2026-03-28
- Status: Approved
- Related task: `18.2 / Define OpenAPI source of truth and generation inputs`

## 1) Authoritative OpenAPI Source

### Decision
The **backend API OpenAPI document** is the single source of truth for client generation.

### Canonical source endpoint
- `http://localhost:5000/openapi/v1.json` (local host execution against `api` service)

### Why backend API (not gateway) is authoritative
1. OpenAPI is emitted directly from API controllers/contracts (`AddOpenApi` + `MapOpenApi`) and versioned explorer metadata.
2. Gateway (`Ocelot`) is a routing layer; it does not own request/response schemas and may aggregate or filter routes.
3. Contract evolution and DTO ownership remain in the backend Application/API layers, consistent with Clean Architecture boundaries.

## 2) Versioning Convention for Spec Input

- OpenAPI document name: `v1`
- Document URL pattern: `/openapi/{documentName}.json`
- Current generation input target: `/openapi/v1.json`

When a new API version is introduced (for example `v2`), generation must explicitly select that document instead of inferring latest.

## 3) Pinned Generation Inputs

### Generator toolchain pinning
- Generator runtime image: `openapitools/openapi-generator-cli:v7.14.0`
- Generator target: `typescript-angular`
- Angular compatibility target: `19.2.0`

### Checked-in configuration
- `frontend/openapi/openapi-generator.config.json`

### Deterministic local command
From repository root:

```bash
cd frontend && ./scripts/generate-openapi-client.sh
```

Equivalent with explicit source override:

```bash
cd frontend && OPENAPI_SPEC_URL=http://localhost:5000/openapi/v1.json ./scripts/generate-openapi-client.sh
```

## 4) Generated Artifact Boundary

- Output folder: `frontend/src/app/core/api/generated/`
- Policy: Generated files are machine-produced artifacts and must not be hand-edited.

## 5) Preconditions for Successful Generation

1. Backend API must run in `Development` environment so `/openapi/v1.json` is exposed.
2. The API should be reachable at `http://localhost:5000` (as mapped in `docker-compose.yml`).
3. Docker must be available locally to execute the pinned OpenAPI Generator image.

## 6) Repeatability Contract

A generation run is considered reproducible when all of the following are true:

1. Source URL is explicitly provided or defaults to `http://localhost:5000/openapi/v1.json`.
2. Generator image tag remains pinned to `v7.14.0`.
3. Config file remains committed and used via `--config /local/openapi/openapi-generator.config.json`.
4. Output is always written to `src/app/core/api/generated` after clean folder reset.
