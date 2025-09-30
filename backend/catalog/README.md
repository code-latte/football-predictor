# FootballCatch.Catalog

Microservicio de **catálogo de competiciones y equipos**.

---

## Eventos emitidos
- `competition.created.v1`
- `competition.updated.v1`
- `team.upserted.v1`


## Eventos consumidos
- Ninguno


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
