# FootballCatch Backend

This is where the .NET modular monolith that makes up the system backend lives.

---

## Project conventions

Each module must comply with:

- **Folder structure**

  ```
  /src/      # Module code (Domain / Application / Infrastructure / Api / Migrations)
  /tests/    # Unit and integration tests
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
  - Domain events handled by `IEventDispatcher`; cross-module integration events published in-process via `IEventPublisher` (see `backend/common/Messaging`).

- **Tests**
  - Unit tests (minimum 70% coverage on domain).
  - Contract tests (for integration events).
  - Minimal DB integration test (NUnit + Testcontainers optional).

---

## Solution layout

All modules are referenced from a single `FootballCatch.sln` at `backend/` root, with solution folders mirroring the physical module folders. There is no per-module `.sln` file.

---

## Current modules

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

- **Application events**: versioned (`.v1`, `.v2`, …) and defined in Common's `Contracts`. Routed in-process by the dispatcher pattern. Both `IEventDispatcher` and `IEventPublisher` have a single shared concrete implementation in `backend/common/Messaging` (`EventDispatcher`, `EventPublisher`). Modules contribute handlers (`IDomainEventHandler<T>`, `IIntegrationEventHandler<T>`) only — they do not implement the dispatcher or publisher themselves.
- **Persistence**: shared PostgreSQL database, table prefix per module to avoid collisions (see `docs/adr/0007-shared-database-shared-schema.md`).
