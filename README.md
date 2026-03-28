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
