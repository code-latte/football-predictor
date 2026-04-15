# FootballCatch.Catalog

Microservice for the **competition and team catalogue**.

---

## Emitted events
- `competition.created.v1`
- `competition.updated.v1`
- `team.upserted.v1`


## Consumed events
- None


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
