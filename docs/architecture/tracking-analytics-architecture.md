# Tracking Analytics Architecture

## Layer Boundaries

- **Domain** keeps tracking invariants in aggregates/value objects (`TrackingPageAggregate`, `VisitEventEntity`, slug/url/tracking identifiers).
- **Application** owns CQRS command/query handlers, validation, DTO contracts, and mapping profiles.
- **Infrastructure/API** contains EF Core persistence repositories, API controllers, and ProblemDetails integration.
- **Gateway** contains Ocelot route and auth-forwarding concerns only.
- **Frontend** uses generated proxies for all tracking and analytics API interactions.

## Core Request Flows

1. **Admin management flow**
   - Authenticated caller hits tracking CRUD endpoints.
   - API controller forwards request to MediatR command/query.
   - Application handlers enforce validation and authorization policies.
   - Repositories persist/read tracking page state.

2. **Public visit flow**
   - Landing endpoint resolves tracking page by slug.
   - Visit ingestion endpoint applies deduplication and privacy controls.
   - Visit event persistence records analytics inputs for downstream queries.

3. **Analytics read flow**
   - Analytics endpoints execute read-side query handlers.
   - Summary, trend, top-page, and recent-stream metrics are aggregated from visit events.
   - API returns contract-stable DTOs exposed through Swagger and generated proxies.

## Performance-sensitive components

- Visit ingestion deduplication lookup (`ExistsDuplicateAsync`) is the critical write-path guard.
- Analytics overview/page handlers parallelize repository reads to reduce end-to-end latency.
- Top-pages aggregation is executed via database-level grouping to avoid loading full candidate sets into memory.
