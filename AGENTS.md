# AGENTS.md

## 🎯 Purpose

This file defines how the AI agent (Codex) must operate while building the QPhising system.

---

## 🧠 GENERAL RULES

* Always follow Clean Architecture
* Never mix layers
* Do not write business logic in controllers
* All operations must go through CQRS (MediatR)
* Always use Repository + UnitOfWork for DB writes
* Use AutoMapper for mapping
* Use FluentValidation for validation

---

## ⚙️ BACKEND RULES

* Domain layer must be pure (no dependencies)
* Application layer must contain CQRS logic only
* Infrastructure handles DB, Redis, external services
* API layer only exposes endpoints

---

## 🎨 FRONTEND RULES

* Use Angular with feature-based structure
* Use PrimeNG components
* Use TailwindCSS for styling
* Separate smart and dumb components
* No inline styles unless necessary

---

## 🔐 SECURITY RULES

* All endpoints must be secured via JWT (Keycloak)
* Role-based authorization is mandatory
* Validate all inputs

---

## ⚡ REDIS RULES

* Use Redis only for:

  * caching
  * rate limiting
  * deduplication
* Never store permanent data in Redis

---

## 🧠 TASK EXECUTION RULES

* Always follow TASKS.md
* Do not skip tasks
* After completing a task, update TASKS.md
* Mark tasks as completed correctly

---

## 🚨 CODING QUALITY RULES

* Do NOT generate placeholder code
* Do NOT leave TODOs
* Code must be production-ready
* Use meaningful naming

---

## 🐳 DEVOPS RULES

* Every service must have a Dockerfile
* docker-compose must run full system
* Environment variables must map to appsettings.json

---

## 📊 UI/UX RULES

* UI must look like enterprise SaaS
* Use consistent spacing and layout
* Dashboard must include charts and KPI cards

---

## 🚀 FINAL RULE

Act like a senior engineer working in a real company.
Do not cut corners.
