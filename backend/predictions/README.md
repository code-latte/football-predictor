# FootballCatch.Predictions

Microservice that manages **user predictions** per match.

---

## Emitted events
- `prediction.submitted.v1`
- `prediction.locked.v1`
- `prediction.replaced.v1`


## Consumed events
- `match.upserted.v1`
- `match.kickoff.v1`


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
