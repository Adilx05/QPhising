# OpenAPI Client Regeneration and Drift-Control Workflow

- Date: 2026-03-28
- Status: Approved
- Related task: `18.2 / Define regeneration and drift-control workflow`

## 1) Purpose

Define a deterministic workflow for regenerating the frontend OpenAPI client and preventing drift between backend contracts and committed generated artifacts.

## 2) Regeneration Triggers (When regeneration is mandatory)

Regenerate the client whenever **any** of the following occur:

1. Backend API contract changes impacting OpenAPI output (controller routes, request/response DTOs, validation metadata, enum/value changes).
2. OpenAPI generation configuration changes (`frontend/openapi/openapi-generator.config.json`).
3. Generator toolchain pin changes (`openapitools/openapi-generator-cli` image tag).
4. Release milestones that publish API-affecting changes (mandatory pre-release refresh gate).

If no trigger is present, regeneration is optional.

## 3) Local SOP (developer workflow)

From repository root:

```bash
cd frontend
npm run generate:api-client
npm run check:api-client-freshness
```

Expected outcome:
- Generation rewrites `src/app/core/api/generated` using pinned config/toolchain.
- Freshness check passes with zero diff under generated artifacts.

If freshness check fails, developer must:
1. Re-run generation against the correct source (`OPENAPI_SPEC_URL` if needed).
2. Commit regenerated artifacts together with the contract change.
3. Re-run freshness check until pass.

## 4) CI SOP (automated drift gate)

CI must run:

```bash
cd frontend
npm run check:api-client-freshness
```

This command is a **required gate** for pull requests that touch backend API contracts, OpenAPI config, or generated client artifacts.

## 5) Freshness Validation Rule (failure criteria)

The workflow is considered failed when the freshness check detects any uncommitted diff under:

- `frontend/src/app/core/api/generated/`

Failure message indicates generated artifacts are stale relative to current OpenAPI contract/config and instructs to regenerate + commit.

## 6) Determinism Controls

1. Pinned generator image (`openapitools/openapi-generator-cli:v7.14.0`).
2. Pinned generator config file committed in repo.
3. Generation script clears output folder before generation.
4. Freshness check enforces zero-drift state after regeneration.

## 7) Ownership and Policy

- Generated artifacts are machine-owned outputs and must not be hand-edited.
- Backend/API contributors are responsible for keeping generated artifacts in sync when contract changes are introduced.
- Reviewers should reject PRs where freshness check fails or generated output is missing for contract changes.
