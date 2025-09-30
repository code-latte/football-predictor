# FootballCatch.Fixtures

Microservicio de **calendario y resultados**: ingesta de partidos desde proveedor externo.

---

## Eventos emitidos
- `match.upserted.v1`
- `match.kickoff.v1`
- `match.scoreChanged.v1`
- `match.finalized.v1`


## Eventos consumidos
- `competition.created.v1`
- `team.upserted.v1`


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
