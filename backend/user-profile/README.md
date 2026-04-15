# FootballCatch.UserProfile

Microservice that manages the **user profile**: nickname, avatar, preferences, privacy.

---

## Emitted events
- `profile.updated.v1`


## Consumed events
- `identity.user.registered.v1`


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
