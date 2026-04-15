# FootballCatch.Stats

Microservice for **global statistics and rankings**.

---

## Emitted events
- `leaderboard.updated.v1`
- `leaderboard.kpi.updated.v1`


## Consumed events
- `scoring.userScoreUpdated.v1`
- `league.tableUpdated.v1`


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
