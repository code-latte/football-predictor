# Infrastructure

## Hosting
- Ubuntu VPS.  
- Docker & Docker Compose for service orchestration.  

## Core Components
- **PostgreSQL**: single database, shared schema. Module isolation is enforced in code (table-name prefixes per module).
- **Redis**: caching and distributed locks.  
- **Nginx**: reverse proxy and SSL termination.  
- **Prometheus**: metrics collection.  
- **Grafana**: dashboards and visualization.  
- **Serilog**: structured logging to console and files.

> Note: the existing RabbitMQ container in `infra/docker-compose.yml` and `infra/rabbit-definitions.json` will be removed in a follow-up PR alongside the in-memory dispatcher implementation. It is no longer part of the target architecture.

## Deployment Workflow
1. Build Docker images in GitHub Actions.  
2. Push to Docker Hub.  
3. Pull and deploy on VPS with `docker compose up -d`.  

## Monitoring & Alerts
- Prometheus scrapes `/metrics` from the backend.
- Grafana dashboards per module.
- Alerts for service down and DB health.

---
