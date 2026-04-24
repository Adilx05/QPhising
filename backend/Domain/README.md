# Domain Modules

Bounded-context folders under this directory define domain ownership boundaries and keep module invariants isolated.

Modules:
- Campaign
- Templates
- Tracking
- Identity
- Gateway
- Persistence
- Common
- ApiConventions
- Contracts

Bu liste klasör yapısı değiştikçe güncellenmelidir.

Contract-source rules are documented in `Contracts/ModuleContractBoundaries.md`.

API naming/version conventions are documented in `Contracts/ApiResourceNamingAndVersionScope.md`.

Persistence aggregate mapping rules are documented in `Contracts/PersistenceAggregateMappings.md`.
