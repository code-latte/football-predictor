# FootballCatch.ScoringEngine

Microservicio encargado del **cálculo de puntos** por predicción y totales de usuario.

---

## Eventos emitidos
- `scoring.userScoreUpdated.v1`
- `scoring.matchScored.v1`


## Eventos consumidos
- `prediction.submitted.v1`
- `match.scoreChanged.v1`
- `match.finalized.v1`


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
