# Microservices Map

## Services

- **Auth** → user authentication, tokens, device registration.  
- **User Profile** → nickname, avatar, user settings.  
- **Catalog** → competitions and teams.  
- **Fixtures** → matches, scores, ingestion from provider.  
- **Predictions** → user match predictions.  
- **Scoring Engine** → calculates points and totals.  
- **Leagues** → private and public leagues.  
- **Stats** → leaderboards, KPIs, rankings.  
- **Notifications** → push notifications and emails.

## Communication
- REST for synchronous APIs.  
- RabbitMQ for events (publish/subscribe).  

## Deployment
- Containerized via Docker.  
- VPS-based hosting, with Docker Compose for orchestration.  
- CI/CD using GitHub Actions.

---
