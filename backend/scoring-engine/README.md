# FootballCatch.ScoringEngine

Microservice responsible for **point calculation** per prediction and user totals.

---

## Emitted events
- `scoring.userScoreUpdated.v1`
- `scoring.matchScored.v1`


## Consumed events
- `prediction.submitted.v1`
- `match.scoreChanged.v1`
- `match.finalized.v1`


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
