# Transactional Consistency Rules

## Purpose

Defines application-layer expectations for write consistency across repositories.

## Rules

1. Commands that mutate persisted state must implement `ITransactionalRequest<TResponse>`.
2. Mutating requests must execute through `IUnitOfWork` via `UnitOfWorkBehavior`.
3. Repository implementations must not perform cross-request shared writes outside the unit-of-work boundary.
4. A unit-of-work implementation may serialize writes when backing stores cannot provide native transactions.

## Current baseline

`EfCoreUnitOfWork` executes transactional requests through a single scoped EF Core `DbContext` and commits tracked changes at the unit-of-work boundary.
