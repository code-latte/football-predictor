# FootballCatch.Fixtures

Microservice for the **match calendar and results**: ingests matches from an external provider.

---

## Emitted events
- `match.upserted.v1`
- `match.kickoff.v1`
- `match.scoreChanged.v1`
- `match.finalized.v1`


## Consumed events
- `competition.created.v1`
- `team.upserted.v1`


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
