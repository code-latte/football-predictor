# Infrastructure

## Hosting
- Ubuntu VPS.  
- Docker & Docker Compose for service orchestration.  

## Core Components
- **RabbitMQ**: message broker.  
- **PostgreSQL**: per-service databases.  
- **Redis**: caching and distributed locks.  
- **Nginx**: reverse proxy and SSL termination.  
- **Prometheus**: metrics collection.  
- **Grafana**: dashboards and visualization.  
- **Serilog**: structured logging to console and files.

## Deployment Workflow
1. Build Docker images in GitHub Actions.  
2. Push to Docker Hub.  
3. Pull and deploy on VPS with `docker compose up -d`.  

## Monitoring & Alerts
- Prometheus scrapes `/metrics` from all services.  
- Grafana dashboards per service.  
- Alerts for service down, RabbitMQ queue saturation, DB health.

---
