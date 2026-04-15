# football-predictor

Microservices-based football prediction platform, with mobile and web applications.  
This repository contains **the entire ecosystem**:

- **Backend**

  - .NET 8 microservices with clean architecture (DDD + CQRS).
  - RabbitMQ as the event bus.
  - PostgreSQL per microservice (one DB per bounded context).
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
/backend/           # .NET microservices
/frontend/
  web/              # Next.js web
  app/              # React Native
  backoffice/       # Next.js backoffice
  common/           # shared npm package
/docs/              # Architecture documentation, ADRs, etc.
/infra/             # Docker, Postgres, RabbitMQ
```

---

## Architecture overview

- Each microservice is **autonomous**: it has its own logical repo inside `/backend/` with its own DB and event contracts.
- Communication is:
  - **Synchronous** → REST calls to the BFF/Gateway.
  - **Asynchronous** → events over RabbitMQ (fanout/topic).
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

- RabbitMQ
- PostgreSQL
- Redis
- Prometheus + Grafana
