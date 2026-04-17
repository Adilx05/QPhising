# AGENTS.md

# 🎯 Purpose

This file defines how the AI agent must operate while building and maintaining the QPhising system.

The repository must be developed with production-grade engineering discipline, backend-first delivery, and strict adherence to architecture boundaries.

---

# 🚀 Core Operating Principle

Act like a senior engineer working in a real company.

Do not cut corners.  
Do not generate throwaway code.  
Do not optimize for speed at the cost of maintainability.  
Prefer correct structure over shortcuts.

---

# 🧠 GENERAL RULES

- Always follow Clean Architecture
- Never mix layers
- Keep responsibilities clear and isolated
- Prefer maintainability over cleverness
- Use consistent naming conventions
- Keep code readable and predictable
- Refactor when duplication appears
- Do not leave dead code behind
- Do not silently ignore errors

---

# 🧱 ARCHITECTURE RULES

The solution must preserve clear layer boundaries.

## Domain
- Must remain pure
- No framework dependencies
- No infrastructure references
- Contains entities, value objects, enums, domain rules

## Application
- Contains use cases only
- CQRS handlers
- Interfaces / contracts
- Validation pipeline logic
- Authorization behaviors when appropriate

## Infrastructure
- Database access
- Redis
- External integrations
- File storage
- Email services
- Keycloak integrations
- Concrete implementations of interfaces

## API
- Exposes HTTP endpoints only
- No business logic in controllers
- Handles request/response transport concerns only

## Gateway
- Ocelot gateway concerns only
- Routing
- Auth forwarding
- Cross-cutting gateway middleware

---

# ⚙️ BACKEND RULES

- All operations must go through CQRS (MediatR)
- Do not place business logic in controllers
- Use FluentValidation for request validation
- Use AutoMapper where useful and maintainable
- Use Repository + UnitOfWork for write operations when repository pattern is part of the solution design
- Keep handlers focused and small
- Use cancellation tokens
- Use async I/O properly
- Use structured logging
- Use ProblemDetails for errors
- Swagger/OpenAPI must remain functional
- Add health checks for critical dependencies

---

# 📄 API CONTRACT RULES

Backend contracts are the source of truth.

For every feature:

1. Domain model
2. Application logic
3. API endpoint
4. Swagger exposure
5. Generate frontend proxies
6. Frontend integration

Do not reverse this order.

If an endpoint is missing, frontend work for that feature must not start.

---

# 🎨 FRONTEND RULES

- Use Angular with feature-based structure
- Use PrimeNG components
- Use TailwindCSS for styling
- Separate smart and dumb components when beneficial
- Use reusable shared UI patterns
- Use route guards where needed
- Keep forms strongly validated
- Keep state manageable and predictable
- No inline styles unless necessary
- No random one-off component patterns
- UI must be responsive

---

# 🚫 FRONTEND BLOCKING RULE

Frontend implementation for a feature is BLOCKED unless all conditions are true:

- Related backend endpoint exists
- Backend builds successfully
- Swagger includes the endpoint
- Generated proxy exists
- Required DTO/models are generated

If any condition fails:

STOP frontend work.  
Explain what is missing.  
Wait for backend/proxy completion.

---

# 🔌 PROXY / SWAGGER RULES

Generated API clients must be used whenever available.

Examples:
- SetupControllerProxy
- CampaignProxy
- AuthProxy

Never create handwritten duplicate services if generated proxies exist.

If backend contracts change:

1. Regenerate proxies
2. Fix compile issues
3. Continue frontend work

Do not continue with stale contracts.

---

# ❌ FORBIDDEN FRONTEND BEHAVIOR

Never do these unless explicitly requested for isolated testing purposes:

- Dummy dashboard statistics
- Fake KPI cards
- Hardcoded API responses
- Temporary arrays pretending to be real data
- Mock users in production flows
- Imaginary forms with fields not backed by contracts
- Fake success flows
- Fake loading states disconnected from real calls
- Manual duplicate API services when proxies exist

---

# 🧙 SETUP WIZARD RULES

QPhising uses a first-run setup wizard.

Until setup is complete:

- Main application must be inaccessible
- Redirect users to setup flow
- Only setup endpoints should be available where applicable

Setup wizard must use real backend APIs via generated proxies.

No fake local-only setup flow.

Runtime configuration must be persisted securely.

---

# 🔐 SECURITY RULES

- All protected endpoints must be secured via JWT (Keycloak)
- Role-based authorization is mandatory
- Validate all inputs
- Never trust client input
- Protect secrets appropriately
- Use least-privilege principles
- Avoid leaking internal errors to clients
- Log security-relevant events where useful

Standard roles:

- Admin
- Operator
- Viewer

---

# ⚡ REDIS RULES

Use Redis only for appropriate transient concerns such as:

- caching
- rate limiting
- deduplication
- short-lived counters
- temporary distributed coordination

Never store permanent source-of-truth business data in Redis.

System must still behave predictably when Redis is disabled unless Redis is explicitly required for that feature.

---

# 🗄️ DATABASE RULES

- Use EF Core migrations properly
- Keep schema changes traceable
- Use transactions where needed
- Seed only intentional bootstrap/sample data
- Avoid destructive changes without migration strategy
- Index frequently queried paths where appropriate
- Keep queries efficient
- Prevent N+1 issues

---

# 🧠 TASK EXECUTION RULES

Always follow TASKS.md.

- Do not skip tasks
- Execute tasks in order unless explicitly changed
- Only one task should be in progress at a time
- Mark statuses correctly
- Update TASKS.md after completion
- Add meaningful notes/evidence
- If blocked, document blocker clearly

Status meanings:

- [ ] Pending
- [-] In Progress
- [x] Completed

---

# ✅ TASK COMPLETION RULE

A task is not complete unless all relevant items pass:

- Code builds successfully
- Related runtime behavior works
- Swagger reflects contract changes
- Proxies are synced
- Frontend compiles
- No placeholder code remains
- No fake data remains
- TASKS.md updated

---

# 🚨 CODING QUALITY RULES

- Do NOT generate placeholder code
- Do NOT leave TODOs as substitute for implementation
- Code must be production-ready
- Use meaningful naming
- Prefer explicitness over ambiguity
- Keep methods cohesive
- Keep classes focused
- Remove unused code
- Add comments only when they provide real value

---

# 📊 UI/UX RULES

- UI must feel like enterprise SaaS software
- Use consistent spacing and layout
- Respect typography hierarchy
- Use accessible interactions where practical
- Dashboard should support charts and KPI cards when real data exists
- Dark mode support is desirable
- Avoid cluttered screens
- Prefer clarity over flashy visuals

---

# 🖥️ RUNTIME RULES

Current target runtime is Windows-native execution.

- Do not require Docker unless explicitly requested
- Keep local run scripts practical
- Support multi-startup development flow for API + Gateway
- Keep configuration manageable

---

# 🐳 OPTIONAL CONTAINER RULES

If containerization is requested later:

- Every service should have a proper Dockerfile
- Compose should support full system startup
- Environment variables should map cleanly to configuration

Do not prioritize this unless requested.

---

# 🧪 TESTING RULES

Where appropriate:

- Add unit tests for core logic
- Add integration tests for important flows
- Prefer testing business rules over trivial code
- Prevent regressions in critical modules

---

# 🤝 DECISION RULE WHEN UNSURE

If uncertain:

1. Prefer backend contract first
2. Prefer simpler maintainable design
3. Prefer explicit code over magic
4. Prefer real integration over fake simulation
5. Prefer documenting blockers over guessing

---

# 🏁 FINAL RULE

Build software that a real company could run confidently in production.
