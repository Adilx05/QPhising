# QPhising 🚀

Enterprise-grade phishing simulation platform.

## 📌 Overview

QPhising is a full-stack SaaS application designed for internal security awareness and phishing simulation.

## 🧱 Architecture

* **Frontend:** Angular + PrimeNG + TailwindCSS
* **Backend:** .NET 10 Web API (Clean Architecture + CQRS)
* **Gateway:** Ocelot
* **Authentication:** Keycloak
* **Database:** PostgreSQL
* **Cache:** Redis
* **Background Jobs:** Worker Service / Hangfire

## ⚙️ Configuration

Backend (`backend/API`), Gateway (`gateway`), and Worker (`worker`) use strongly typed options with startup validation.  
Required keys are loaded from `appsettings.json` and can be overridden by environment variables.

### Backend configuration keys

| Section | Key | Env override example |
|---|---|---|
| `Database` | `ConnectionString` | `Database__ConnectionString` |
| `Redis` | `ConnectionString` | `Redis__ConnectionString` |
| `Keycloak` | `Authority`, `Audience`, `RequireHttpsMetadata` | `Keycloak__Authority` |
| `Smtp` | `Host`, `Port`, `Username`, `Password`, `FromAddress`, `UseSsl` | `Smtp__Host` |
| `BaseUrls` | `Gateway`, `Frontend` | `BaseUrls__Gateway` |
| `TrackingTokens` | `SigningKey`, `ExpirationMinutes`, `Version` | `TrackingTokens__SigningKey` |
| `TrackingRetention` | `RawClickRetentionDays`, `AggregateRetentionDays`, `CleanupIntervalMinutes`, `CleanupBatchSize` | `TrackingRetention__RawClickRetentionDays` |
| `ExportStorage` | `BasePath`, `FileTtlDays` | `ExportStorage__BasePath` |
| `ExportRetention` | `CleanupIntervalMinutes`, `CleanupBatchSize` | `ExportRetention__CleanupIntervalMinutes` |
| `Redis` | `TrackingDeduplicationWindowMinutes`, `TrackingTokenClockSkewSeconds`, `TrackingAbuseWindowMinutes`, `TrackingSuspiciousIpThreshold`, `TrackingIpRejectionThreshold`, `TrackingAggregateRetentionDays`, `AnalyticsDashboardCacheTtlSeconds` | `Redis__TrackingDeduplicationWindowMinutes` |

### Gateway configuration keys

| Section | Key | Env override example |
|---|---|---|
| `Database` | `ConnectionString` | `Database__ConnectionString` |
| `Redis` | `ConnectionString` | `Redis__ConnectionString` |
| `Keycloak` | `Authority`, `Audience`, `RequireHttpsMetadata` | `Keycloak__Authority` |
| `Smtp` | `Host`, `Port`, `Username`, `Password`, `FromAddress`, `UseSsl` | `Smtp__Host` |
| `BaseUrls` | `Gateway`, `Api`, `Frontend` | `BaseUrls__Api` |

### Worker configuration keys

| Section | Key | Env override example |
|---|---|---|
| `Database` | `ConnectionString` | `Database__ConnectionString` |
| `Redis` | `ConnectionString`, `TrackingDeduplicationWindowMinutes`, `TrackingTokenClockSkewSeconds`, `TrackingAbuseWindowMinutes`, `TrackingSuspiciousIpThreshold`, `TrackingIpRejectionThreshold`, `TrackingAggregateRetentionDays` | `Redis__ConnectionString` |
| `TrackingTokens` | `SigningKey`, `ExpirationMinutes`, `Version` | `TrackingTokens__SigningKey` |
| `TrackingRetention` | `RawClickRetentionDays`, `AggregateRetentionDays`, `CleanupIntervalMinutes`, `CleanupBatchSize` | `TrackingRetention__CleanupBatchSize` |
| `ExportStorage` | `BasePath`, `FileTtlDays` | `ExportStorage__BasePath` |
| `ExportRetention` | `CleanupIntervalMinutes`, `CleanupBatchSize` | `ExportRetention__CleanupBatchSize` |
| `TaskWorker` | `PollIntervalSeconds`, `ClaimLeaseDurationSeconds`, `InitialRetryDelaySeconds`, `MaxRetryDelaySeconds`, `RetryBackoffMultiplier` | `TaskWorker__PollIntervalSeconds` |

> The applications fail fast at startup if required keys are missing or invalid.

### Production-safe templates

Copy and fill these templates before production deployment:

* `backend/API/appsettings.Production.Template.json`
* `gateway/appsettings.Production.Template.json`

## 🐳 Run with Docker

Use environment overrides (optional) and start the stack:

```bash
docker compose up --build
```

`docker-compose.yml` now uses `${VAR:-default}` expansion for runtime values (DB, Redis, Keycloak, SMTP, base URLs, tracking, export, and worker execution tuning).

### Full-stack lifecycle verification

Use the verification script to run one-command startup, wait for health convergence, and validate endpoint reachability:

```bash
./scripts/compose/verify-full-stack-lifecycle.sh
```

The script performs:

* `docker compose up --build -d --remove-orphans`
* health convergence checks using `docker compose ps --format json`
* reachability probes for Keycloak, API, Gateway, and Frontend
* automatic cleanup via `docker compose down -v --remove-orphans`

Optional overrides:

* `STARTUP_TIMEOUT_SECONDS` (default `600`)
* `POLL_INTERVAL_SECONDS` (default `5`)
* `*_HEALTH_URL` variables for custom endpoint probes

### Compose operational profiles

Use these profiles to standardize runtime behavior by environment:

#### 1) Local development profile (interactive logs)

Use this while actively developing and debugging service interactions.

```bash
docker compose up --build
```

When finished:

```bash
docker compose down --remove-orphans
```

#### 2) Local detached profile (background stack)

Use this for longer-lived local sessions where terminal access should stay free.

```bash
docker compose up --build -d --remove-orphans
docker compose ps
```

Follow logs for specific services when needed:

```bash
docker compose logs -f api gateway worker
```

Tear down and remove volumes for a clean reset:

```bash
docker compose down -v --remove-orphans
```

#### 3) CI/verification profile (health-gated lifecycle)

Use the repository verification script to enforce startup + health convergence + reachability in one command:

```bash
./scripts/compose/verify-full-stack-lifecycle.sh
```

Recommended for pull-request validation and pre-release smoke checks.

#### 4) Deployment-like profile (explicit runtime overrides)

Use explicit environment overrides (or an external `.env` file) to align runtime settings with target infrastructure without image rebuilds.

```bash
Database__ConnectionString="Host=postgres;Port=5432;Database=qphising;Username=qphising;Password=change-me" \
Keycloak__Authority="http://keycloak:8080/realms/qphising" \
BaseUrls__Gateway="http://gateway:8080" \
docker compose up -d --build
```

### Compose troubleshooting notes

* **Port collisions (`bind: address already in use`)**: stop conflicting local services or remap host ports in `docker-compose.yml`.
* **Gateway/API 401 or auth metadata errors at startup**: confirm `keycloak` is healthy (`docker compose ps`) and `Keycloak__Authority` matches compose networking hostnames.
* **Service marked `unhealthy`**: inspect targeted logs (`docker compose logs <service>`) and validate required env mappings from the configuration matrices above.
* **Stale state between runs**: run `docker compose down -v --remove-orphans` before restarting to clear outdated volumes/networks.
* **Slow first startup**: initial image builds and Keycloak/PostgreSQL initialization may take several minutes; use the lifecycle script timeout overrides for constrained hosts.

### Deterministic image build assumptions

To keep image outputs reproducible across environments:

* Dockerfiles use pinned base image tags for Node, NGINX, and .NET SDK/runtime.
* Frontend build uses lockfile-driven installs via `npm ci` (not `npm install`).
* .NET publish stages enable deterministic CI build flags (`ContinuousIntegrationBuild=true`, `Deterministic=true`).
* Build contexts are constrained using per-service `.dockerignore` files to avoid non-source noise affecting layers.

## 📊 Task System

All development tasks are tracked in:

```text
TASKS.md
```

## 🤖 AI Development

Rules for AI are defined in:

```text
AGENTS.md
```

## 🚀 Status

Active development (AI-driven)
