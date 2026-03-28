# OpenAPI Generated Client Architecture Compatibility Checklist

- Date: 2026-03-28
- Status: Approved
- Related task: `18.2 / Validate architecture compatibility of generated clients`
- Depends on:
  - `docs/openapi-generation-strategy-decision.md`
  - `docs/openapi-generated-client-module-structure.md`
  - `docs/openapi-regeneration-and-drift-control-workflow.md`

## 1) Objective

Validate that generated OpenAPI clients can be adopted in the current Angular architecture without violating clean layering. This checklist confirms compatibility for:

1. Authentication interceptor flow.
2. Typed error handling.
3. Centralized API consumption through facades.

It also defines approved extension points and required wrapper/facade strategy.

## 2) Compatibility Checklist

### A. Auth interceptor compatibility

| Check | Requirement | Result | Notes |
|---|---|---|---|
| A1 | Generated clients execute through Angular `HttpClient` pipeline. | ✅ Pass | `typescript-angular` generated clients call injected Angular `HttpClient`, so registered interceptors remain active. |
| A2 | Bearer token attachment is centralized and not duplicated in generated classes. | ✅ Pass | Token policy stays in global auth interceptor; generated files remain transport-only and stateless. |
| A3 | Unauthorized/forbidden responses can trigger auth policies (refresh/logout/redirect). | ✅ Pass | Interceptor-level handling remains authoritative for `401/403`, independent from feature code. |
| A4 | Public endpoint calls can bypass token where policy requires. | ✅ Pass | Auth interceptor can conditionally skip header injection using request URL/policy metadata. |

### B. Typed error handling compatibility

| Check | Requirement | Result | Notes |
|---|---|---|---|
| B1 | API failures are normalized into typed app-level error contract. | ✅ Pass | Required `core/api/errors/api-error.mapper.ts` maps `HttpErrorResponse` + ProblemDetails to typed domain errors. |
| B2 | Feature code avoids direct transport error-shape branching. | ✅ Pass | Facades must translate raw errors before exposing observables to features. |
| B3 | Validation errors preserve field-level information. | ✅ Pass | Mapper contract includes `validationErrors` projection from ProblemDetails `errors` object. |
| B4 | Unknown errors degrade safely with deterministic fallback type. | ✅ Pass | Error mapper defines `UnknownApiError` fallback with correlation/trace preservation when available. |

### C. Centralized API consumption compatibility

| Check | Requirement | Result | Notes |
|---|---|---|---|
| C1 | Feature modules consume only facade APIs, not generated services directly. | ✅ Pass | Enforced by boundary rule from module-structure blueprint. |
| C2 | DTO-to-UI transformations are centralized and reusable. | ✅ Pass | `core/api/mappers/**` is the single mapping layer for generated DTO conversion. |
| C3 | Generated output can be regenerated without breaking feature imports. | ✅ Pass | Features depend on facade contracts, not generated class names/paths. |
| C4 | Duplicate handwritten HTTP transport code is avoided. | ✅ Pass | Generated client remains sole transport implementation source-of-truth. |

## 3) Approved Extension Points

Only the following handwritten extension points are approved:

1. `core/api/client/api-client.providers.ts`
   - Configure generated `Configuration`, base path, and provider wiring.
2. `core/api/client/api-client.tokens.ts`
   - Define DI tokens for runtime API base URL and API policy switches.
3. Global Angular interceptors (`core/auth` / platform layer)
   - Auth token attach/refresh/logout flow.
   - Correlation/header enrichment when needed.
4. `core/api/errors/api-error.mapper.ts`
   - Transport-to-domain typed error normalization.
5. `core/api/mappers/*.mapper.ts`
   - DTO-to-UI model mapping and null-safe defaults.
6. `core/api/facades/*.facade.ts`
   - Feature-oriented orchestration layer wrapping generated services.

Disallowed extension points:
- Editing files inside `core/api/generated/**`.
- Injecting generated services directly into smart/dumb components.
- Writing ad-hoc `HttpClient` calls for endpoints already represented by OpenAPI-generated services.

## 4) Required Wrapper/Facade Strategy

Each feature must consume backend endpoints through a facade with this flow:

```text
Feature Container/Store
   -> Feature Facade (core/api/facades)
      -> Generated Service (core/api/generated/api)
      -> DTO Mapper (core/api/mappers)
      -> Error Mapper (core/api/errors)
```

Mandatory facade responsibilities:

1. Keep method surface feature-oriented (`getCampaignList`, `activateCampaign`, etc.).
2. Map generated DTOs to UI models before exposing data.
3. Normalize errors into typed app-level error contracts.
4. Prevent transport details from leaking into components/state stores.
5. Keep contracts stable across OpenAPI regeneration cycles.

## 5) Compliance Rules (Must/Must Not)

### Must
1. Must use generated services as the only typed transport source.
2. Must route all feature API calls through facades.
3. Must handle auth concerns in interceptors, not in generated code.
4. Must normalize API errors before they reach presentation/store layers.

### Must not
1. Must not hand-edit generated files.
2. Must not call generated services directly from components.
3. Must not duplicate endpoint calls with handwritten `HttpClient` services.
4. Must not expose raw `HttpErrorResponse` outside API/facade boundaries.

## 6) Verification Commands

```bash
rg -n "HttpClient|http\\.(get|post|put|delete|patch)|fetch\\(|/api/" frontend/src/app -g '*.ts'
rg -n "core/api/generated|core/api/facades|api-error.mapper|api-client.providers" docs/frontend-*.md docs/openapi-*.md
sed -n '1,320p' docs/openapi-generated-client-architecture-compatibility-checklist.md
```

## 7) Final Compatibility Verdict

Generated OpenAPI clients are architecture-compatible with the current frontend when consumed through the defined facade/mapping/error-normalization layers and shared interceptor pipeline. No architecture-blocking incompatibility is identified for auth, typed error handling, or centralized consumption patterns.
