# FootballCatch.Leagues

Microservicio de gestión de **ligas privadas y públicas** entre usuarios.

---

## Eventos emitidos
- `league.created.v1`
- `league.joined.v1`
- `league.left.v1`
- `league.updated.v1`
- `league.tableUpdated.v1`


## Eventos consumidos
- `profile.updated.v1`
- `scoring.userScoreUpdated.v1`
- `identity.user.registered.v1`


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
