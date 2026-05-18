# FootballCatch.Catalog

Module for the **competition and team catalogue**. Cross-module communication is performed in-process via the dispatcher (see `backend/common/Messaging`).

---

## Emitted events
- `competition.created.v1`
- `competition.updated.v1`
- `team.upserted.v1`


## Consumed events
- None


---

## Infrastructure

- Shared PostgreSQL database; tables prefixed with `catalog_`.
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
