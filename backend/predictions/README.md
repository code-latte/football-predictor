# FootballCatch.Predictions

Microservicio que gestiona las **predicciones de los usuarios** por partido.

---

## Eventos emitidos
- `prediction.submitted.v1`
- `prediction.locked.v1`
- `prediction.replaced.v1`


## Eventos consumidos
- `match.upserted.v1`
- `match.kickoff.v1`


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
