# FootballCatch.Leagues

Microservice for managing **private and public leagues** between users.

---

## Emitted events
- `league.created.v1`
- `league.joined.v1`
- `league.left.v1`
- `league.updated.v1`
- `league.tableUpdated.v1`


## Consumed events
- `profile.updated.v1`
- `scoring.userScoreUpdated.v1`
- `identity.user.registered.v1`


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
