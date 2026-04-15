# Architecture Overview

FootballCatch is built with a **microservices architecture** on .NET 10.

## System Context
- **Users**: web, mobile app, and backoffice admins.  
- **External APIs**: football data provider (fixtures & results).  
- **Infrastructure**: RabbitMQ, PostgreSQL, Redis, Nginx, Prometheus, Grafana.

## Containers
- **Microservices**: Auth, User Profile, Catalog, Fixtures, Predictions, Scoring Engine, Leagues, Stats, Notifications.  
- **Databases**: Each microservice has its own PostgreSQL database.  
- **Event Bus**: RabbitMQ for async communication.  
- **Cache**: Redis.  
- **Reverse Proxy**: Nginx as entry point.  
- **Observability**: Prometheus metrics, Grafana dashboards, Serilog structured logging.

## Frontend
- **web**: Next.js app — server-side rendering, predictions UI, rankings.  
- **backoffice**: Next.js admin panel — competition management, moderation.  
- **app**: React Native (Expo) — mobile predictions and push notifications.  
- **common**: Shared npm package (`@footballcatch/common`) implementing clean architecture (domain, application, infrastructure layers). Consumed by all three apps.

## Principles
- DDD + CQRS.  
- Event-driven integration.  
- Each service independently deployable.  
- Strong focus on observability and testability.

---
