# FootballCatch.Stats

Microservicio de **estadísticas y rankings globales**.

---

## Eventos emitidos
- `leaderboard.updated.v1`
- `leaderboard.kpi.updated.v1`


## Eventos consumidos
- `scoring.userScoreUpdated.v1`
- `league.tableUpdated.v1`


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
