# Modules Map

## Modules

- **Auth** → user authentication, tokens, device registration.  
- **User Profile** → nickname, avatar, user settings.  
- **Catalog** → competitions and teams.  
- **Fixtures** → matches, scores, ingestion from provider.  
- **Predictions** → user match predictions.  
- **Scoring Engine** → calculates points and totals.  
- **Leagues** → private and public leagues.  
- **Stats** → leaderboards, KPIs, rankings.  
- **Notifications** → push notifications and emails.  
- **Updater** → background process; polls external football API and syncs data into Catalog and Fixtures via REST. Emits no events itself — Catalog and Fixtures emit events after persisting Updater-driven writes. See `backend/updater/README.md`.

## Communication
- REST for synchronous APIs (until the per-module `*.Api` projects are merged into a single host).
- In-process dispatcher for events (no RabbitMQ). Cross-module integration events are routed through `IEventPublisher<T : IIntegrationEvent>` defined in `backend/common/Messaging`.

## Deployment
- Containerized via Docker. Single backend container.
- VPS-based hosting, Docker Compose for orchestration.
- CI/CD via GitHub Actions.

---
