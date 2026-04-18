# OpenAPI Operation Conventions (Application Layer)

This document defines Application-layer conventions consumed by API/Swagger integration.

## 1. Operation ID convention

Use `OpenApiOperationIdConvention.Create(resourceName, actionName)` to build canonical operation IDs.

Rules:
- Output must be PascalCase.
- Output must start with a letter.
- Resource and action names may be supplied in kebab-case, snake_case, or spaced words.
- Recommended pattern: `{Action}{Resource}`.

Examples:
- `("setup", "get-status") => GetStatusSetup`
- `("runtime-configuration", "save") => SaveRuntimeConfiguration`

## 2. Response envelope convention

Use `ApiResponseEnvelope<TData>` when an endpoint needs a standard success envelope.

Envelope fields:
- `data` (required)
- `message` (optional)
- `correlationId` (optional)

This keeps clients predictable while preserving domain/application DTO ownership.

## 3. OpenAPI example provider convention

Use `IOpenApiExampleProvider<TRequest, TResponse>` (or response-only variant) for centralized example generation.

Rules:
- Examples must be deterministic.
- Examples must reflect real contracts (no fake/unbacked fields).
- Examples should represent a valid success path payload.
