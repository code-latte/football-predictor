# FootballCatch.Auth

Microservice responsible for user **authentication and authorisation** (registration, login, JWT, refresh tokens).

---

## Emitted events
- `identity.user.registered.v1`
- `identity.user.disabled.v1`
- `identity.deviceToken.added.v1`


## Consumed events
- None


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
