# Phase 0 Cleanup Audit (Legacy Phishing/Email Concepts)

Date: 2026-04-19

## Scope audited
- Backend Domain/Application/Infrastructure/API/Gateway source folders
- Frontend application source and generated OpenAPI fixture/proxy surface
- Top-level project docs (`README.md`)

## Findings
1. **UI routing/navigation still exposed legacy Campaign/Template workflows** via `/campaigns` and `/templates`.
2. **User-facing product wording** still referred to a generic security operations console instead of visitor analytics framing.
3. **Backend contracts and persistence** still include legacy campaign/template/email-target semantics and generated proxy artifacts mirror these contracts.

## Actions completed in this phase
1. Removed legacy campaign/template route exposure from Angular router.
2. Removed campaign/template entries from shell navigation in desktop/mobile layouts.
3. Updated top-level product wording toward visitor analytics positioning.
4. Recorded remaining backend legacy surfaces as explicit carry-over debt to be replaced in subsequent tracking phases (Domain/Application/API redesign around TrackingPage + VisitEvent).

## Remaining legacy surfaces (tracked for upcoming phases)
- `Campaign*` and `Template*` contracts, handlers, controllers, persistence entities/migrations, and generated proxies.
- Email-target workflow references in campaign target models and validators.

These remaining items are intentionally documented so follow-up phases can replace contracts in dependency order (Domain -> Application -> API -> Swagger -> Proxy -> Frontend) without creating stale or handwritten client surfaces.
