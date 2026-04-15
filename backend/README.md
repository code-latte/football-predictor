# FootballCatch Backend

This is where the .NET microservices that make up the system backend live.

---

## Project conventions

Each microservice must comply with:

- **Folder structure**

  ```
  /src/      # Microservice code
  /tests/    # Unit and integration tests
  *.sln      # Solution at the microservice root
  ```

- **Required technologies**

  - **.NET 10**
  - **NUnit** for testing
  - **Serilog** for structured logging
  - **Prometheus** for metrics (endpoint `/metrics`)
  - **Grafana** for visualization (centralised config in `/docs/infra/`)

- **Architecture**

  - Layers: Domain / Application / Infrastructure / API
  - DDD + CQRS: entities, value objects, aggregates.
  - Domain events → converted into integration events for RabbitMQ.

- **Tests**
  - Unit tests (minimum 70% coverage on domain).
  - Contract tests (for integration events).
  - Minimal DB integration test (NUnit + Testcontainers optional).

---

## Current microservices

- `auth` → Registration, login, auth (JWT).
- `user-profile` → Nicknames, avatars.
- `catalog` → Team, league, and player information.
- `fixtures` → Match calendar and results.
- `predictions` → User prediction management.
- `scoring-engine` → Point and total calculation.
- `leagues` → Private leagues and rankings.
- `notifications` → Push and email.
- `stats` → Global tables and statistics.

---

## Additional guides

- **Integration events**: versioned (`.v1`, `.v2` …) and defined in Common's `Contracts`.
- **Idempotency**: each consumer applies the _Inbox_ pattern.
- **Persistence**: PostgreSQL per service.
