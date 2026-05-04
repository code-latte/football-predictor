# FootballCatch.Updater

Background microservice that fetches data from an external football data API and keeps the platform's competitions, teams, fixtures, live scores, and results in sync.

---

## Responsibilities

- Poll an external football data API on a schedule.
- Sync competitions and teams into the Catalog service.
- Sync upcoming fixtures, live scores, and final results into the Fixtures service.
- Trigger kickoff locking by notifying the Fixtures service when a match moves to `In Progress`.
- Detect match finalisation and notify the Fixtures service to close the scoring window.

The Updater does **not** own any domain data. It is a thin orchestration process: it reads from an external source, translates the payload, and calls the REST APIs of the services that own the relevant bounded contexts. Those services are responsible for persisting and emitting integration events.

---

## Events Emitted

None. The Updater does not publish integration events directly.

Integration events (`competition.created.v1`, `team.upserted.v1`, `match.upserted.v1`, `match.kickoff.v1`, `match.scoreChanged.v1`, `match.finalized.v1`) are emitted by the **Catalog** and **Fixtures** services after the Updater triggers a write through their REST APIs.

## Events Consumed

None. The Updater is not a RabbitMQ consumer.

---

## Scheduled Jobs

| Job | Schedule | Use Case |
|---|---|---|
| `SyncCompetitionsJob` | Daily (configurable) | UC-UP01 — fetch and upsert active competitions |
| `SyncTeamsJob` | Daily (configurable) | UC-UP02 — fetch and upsert teams per competition |
| `SyncFixturesJob` | Daily, start of each matchday | UC-UP03 — fetch and upsert upcoming fixtures |
| `LiveScorePollingJob` | Every 60 s during match windows | UC-UP04 — poll current scores for in-progress matches |
| `FinalizeResultsJob` | Every 60 s during match windows | UC-UP05 — detect finished matches and finalize results |
| `KickoffLockJob` | Every 60 s | UC-UP06 — detect kickoff and trigger prediction lock |

Match window detection (when live polling is active) is driven by the presence of matches in status `Scheduled` that are within the configured pre-kickoff buffer, or matches already in status `In Progress`.

---

## Infrastructure

- Own PostgreSQL database — used to store the last-seen state of each external entity (external ID, last-modified hash, sync cursor) to enable idempotent delta syncing.
- HTTP client to the external football data API (e.g., API-Football, football-data.org).
- HTTP clients to the Catalog service REST API and the Fixtures service REST API.
- Hosted services (`.NET IHostedService` / `BackgroundService`) — one per scheduled job.
- `/metrics` endpoint for Prometheus.
- No RabbitMQ connection — the Updater neither publishes nor consumes events.

---

## Communication with Other Services

The Updater calls the **REST APIs** of the Catalog and Fixtures services. It does not publish to RabbitMQ.

**Rationale:** The Catalog and Fixtures services own the bounded contexts for competitions/teams and matches respectively. Those services validate invariants, persist the data, and emit the authoritative integration events. If the Updater published events directly, it would bypass the owning service's business rules and produce events without the internal state being persisted — violating the "emitter owns the data" principle. Calling the services' REST APIs preserves bounded context ownership and keeps event responsibility unambiguous.

| Target | Operation | Trigger |
|---|---|---|
| Catalog REST API | `PUT /competitions/{externalId}` | `SyncCompetitionsJob` |
| Catalog REST API | `PUT /teams/{externalId}` | `SyncTeamsJob` |
| Fixtures REST API | `PUT /matches/{externalId}` | `SyncFixturesJob` |
| Fixtures REST API | `PUT /matches/{externalId}/score` | `LiveScorePollingJob` |
| Fixtures REST API | `POST /matches/{externalId}/finalize` | `FinalizeResultsJob` |
| Fixtures REST API | `POST /matches/{externalId}/kickoff` | `KickoffLockJob` |

---

## Project Structure

```
/src/
  Updater.Domain/
  Updater.Application/        # Job definitions, IFootballApiClient port, IExternalSyncRepository port
  Updater.Infrastructure/     # HTTP clients, job schedulers (BackgroundService), EF Core
  Updater.Api/                # Minimal host; triggers hosted services, exposes /metrics and /health
  Updater.Migrations/         # EF Core DbContext + migration classes for sync-state tables
/tests/
  Updater.UnitTests/
  Updater.IntegrationTests/
  Updater.Tests.Infrastructure/
Updater.sln
```

---

## Configuration

| Environment variable | Description | Example |
|---|---|---|
| `FOOTBALL_API_BASE_URL` | Base URL of the external football data provider | `https://v3.football.api-sports.io` |
| `FOOTBALL_API_KEY` | API key for the external provider | `abc123` |
| `CATALOG_SERVICE_URL` | Base URL of the Catalog service REST API | `http://catalog-service:8080` |
| `FIXTURES_SERVICE_URL` | Base URL of the Fixtures service REST API | `http://fixtures-service:8080` |
| `SYNC_COMPETITIONS_CRON` | Cron expression for `SyncCompetitionsJob` | `0 3 * * *` (03:00 daily) |
| `SYNC_TEAMS_CRON` | Cron expression for `SyncTeamsJob` | `0 4 * * *` (04:00 daily) |
| `SYNC_FIXTURES_CRON` | Cron expression for `SyncFixturesJob` | `0 6 * * 1` (06:00 every Monday) |
| `LIVE_POLL_INTERVAL_SECONDS` | Polling interval for live score and kickoff jobs | `60` |
| `CONNECTION_STRING` | PostgreSQL connection string for the Updater's own database | `Host=...;Database=updater;...` |

---

## Gotchas

- **External API rate limits.** Most football data providers enforce per-minute and per-day request quotas. Batch requests by competition or matchday; do not call per-match during bulk sync. Log and back off on `429` responses.
- **Idempotency via state hash.** The Updater stores a hash of the last-synced payload per external entity. Only entities whose hash has changed are forwarded to the downstream service, preventing duplicate REST calls and redundant event emissions.
- **Live polling activation.** The `LiveScorePollingJob` and `KickoffLockJob` should only run at elevated frequency when matches are actually in progress. Outside match windows they can run at a reduced cadence (or sleep entirely) to conserve API quota.
- **Clock skew between external API and platform.** External API timestamps may lag by 1–2 minutes relative to actual kickoff. Apply a configurable tolerance window before treating a match as `In Progress`.
- **No direct database access to Catalog or Fixtures.** The Updater must never read from or write to another service's database. All interactions go through REST APIs.
