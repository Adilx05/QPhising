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

Backend (`backend/API`) and Gateway (`gateway`) now use strongly typed options with startup validation.  
Required keys are loaded from `appsettings.json` and can be overridden by environment variables.

### Backend configuration keys

| Section | Key | Env override example |
|---|---|---|
| `Database` | `ConnectionString` | `Database__ConnectionString` |
| `Redis` | `ConnectionString` | `Redis__ConnectionString` |
| `Keycloak` | `Authority`, `Audience`, `RequireHttpsMetadata` | `Keycloak__Authority` |
| `Smtp` | `Host`, `Port`, `Username`, `Password`, `FromAddress`, `UseSsl` | `Smtp__Host` |
| `BaseUrls` | `Gateway`, `Frontend` | `BaseUrls__Gateway` |

### Gateway configuration keys

| Section | Key | Env override example |
|---|---|---|
| `Database` | `ConnectionString` | `Database__ConnectionString` |
| `Redis` | `ConnectionString` | `Redis__ConnectionString` |
| `Keycloak` | `Authority`, `Audience`, `RequireHttpsMetadata` | `Keycloak__Authority` |
| `Smtp` | `Host`, `Port`, `Username`, `Password`, `FromAddress`, `UseSsl` | `Smtp__Host` |
| `BaseUrls` | `Gateway`, `Api`, `Frontend` | `BaseUrls__Api` |

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

`docker-compose.yml` now uses `${VAR:-default}` expansion for runtime values (DB, Redis, Keycloak, SMTP, base URLs, exposed ports).

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
