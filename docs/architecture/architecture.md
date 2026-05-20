# Architecture Overview

FootballCatch is built as a **modular monolith** on .NET 10.

## System Context
- **Users**: web, mobile app, and backoffice admins.  
- **External APIs**: football data provider (fixtures & results).  
- **Infrastructure**: PostgreSQL, Redis, Nginx, Prometheus, Grafana.

## Containers
- **Backend deployable**: a single modular monolith composed of the following modules — Auth, User Profile, Catalog, Fixtures, Predictions, Scoring Engine, Leagues, Stats, Notifications, Updater.
- **Database**: a single PostgreSQL database with a shared schema; tables are prefixed by module short name to keep boundaries collision-free.
- **Cache**: Redis.  
- **Reverse Proxy**: Nginx as entry point.  
- **Observability**: Prometheus metrics, Grafana dashboards, Serilog structured logging.

## Frontend
- **web**: Next.js app — server-side rendering, predictions UI, rankings.  
- **backoffice**: Next.js admin panel — competition management, moderation.  
- **app**: React Native (Expo) — mobile predictions and push notifications.  
- **common**: Shared npm package (`@footballcatch/common`) implementing clean architecture (domain, application, infrastructure layers). Consumed by all three apps.

## Principles
- DDD + CQRS per module.
- In-process event-driven decoupling at the module boundary (dispatcher pattern, see `backend/common/Messaging`).
- Single deployable; module boundaries are enforced in code, not in infrastructure.
- Strong focus on observability and testability.

---
