# FootballCatch.Notifications

Microservicio encargado de enviar **notificaciones push y correos**.

---

## Eventos emitidos
- `notification.sent.v1`


## Eventos consumidos
- `match.kickoff.v1`
- `prediction.locked.v1`
- `scoring.userScoreUpdated.v1`


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
