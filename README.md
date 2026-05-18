# football-predictor

Modular monolith football prediction platform, with mobile and web applications.  
This repository contains **the entire ecosystem**:

- **Backend**

  - .NET 10 modular monolith with clean architecture (DDD + CQRS).
  - In-process event dispatcher for cross-module decoupling.
  - Single PostgreSQL database with a shared schema; module boundaries enforced in code.
  - Redis for caching and locks.
  - Observability with Prometheus, Grafana, and structured logs with Serilog.

- **Frontend**

  - `frontend/web` → Next.js web app (SSR + ISR) for web access to predictions and rankings.
  - `frontend/app` → Mobile app in React Native (Expo).
  - `frontend/backoffice` → Next.js admin panel for competition, rule, and moderation management.
  - `frontend/common` → Shared library with clean architecture (domain + application + infrastructure), published as an npm package.

---

## Repository structure

```
/backend/           # .NET modular monolith (one folder per module)
/frontend/
  web/              # Next.js web
  app/              # React Native
  backoffice/       # Next.js backoffice
  common/           # shared npm package
/docs/              # Architecture documentation, ADRs, etc.
/infra/             # Docker, Postgres
```

---

## Architecture overview

- Each module is a **bounded context** inside the monolith: it has its own folder under `/backend/`, its own domain model, and its own event contracts.
- Module boundaries are enforced in code — modules do not directly query each other's tables. Tables are prefixed with the module short name to keep schemas collision-free.
- Communication is:
  - **Synchronous** → REST calls to the BFF/Gateway.
  - **Asynchronous / decoupled** → in-process events via the dispatcher pattern (see `backend/common/Messaging`).
- CI/CD with **GitHub Actions**: build, test, and deploy to VPS with Docker Compose.

---

## How to run everything locally

```bash
# Start base infrastructure
docker compose -f infra/docker-compose.yml up -d

# Start all services
docker compose up -d
```

Base infrastructure includes:

- PostgreSQL
- Redis
- Prometheus + Grafana
