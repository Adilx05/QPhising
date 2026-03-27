# QPhising 🚀

Enterprise-grade phishing simulation platform.

## 📌 Overview

QPhising is a full-stack SaaS application designed for internal security awareness and phishing simulation.

The system enables organizations to:

* Create phishing campaigns
* Track user interactions
* Analyze behavior
* Generate reports
* Manage tasks and automation

---

## 🧱 Architecture

* **Frontend:** Angular + PrimeNG + TailwindCSS
* **Backend:** .NET 10 Web API (Clean Architecture + CQRS)
* **Gateway:** Ocelot
* **Authentication:** Keycloak
* **Database:** PostgreSQL
* **Cache:** Redis
* **Background Jobs:** Worker Service / Hangfire

---

## ⚙️ Configuration

The system is fully configurable via:

```
appsettings.json
```

No manual setup required.

---

## 🐳 Run with Docker

```bash
docker-compose up --build
```

---

## 🔐 Authentication

* Managed by Keycloak
* JWT-based authentication
* Role-based access control

---

## 🧠 Core Features

* Campaign management
* Tracking & analytics
* Task execution engine
* Export (Excel / PDF)
* Real-time dashboard

---

## 📊 Task System

All development tasks are tracked in:

```
TASKS.md
```

The system evolves step-by-step based on task execution.

---

## 🤖 AI Development

This project is designed to be built and maintained using AI agents.

Rules for AI are defined in:

```
AGENTS.md
```

---

## 🚨 Important Rules

* Clean Architecture is mandatory
* CQRS must be used everywhere
* No business logic in controllers
* No placeholder code

---

## 🚀 Status

Active development (AI-driven)

---

## 👨‍💻 Author

Built as an enterprise internal tool.
