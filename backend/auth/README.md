# FootballCatch.Auth

Module responsible for user **authentication and authorisation** (registration, login, JWT, refresh tokens). Cross-module communication is performed in-process via the dispatcher (see `backend/common/Messaging`).

---

## Emitted events
- `identity.user.registered.v1`
- `identity.user.disabled.v1`
- `identity.deviceToken.added.v1`


## Consumed events
- None


---

## Infrastructure

- Shared PostgreSQL database; tables prefixed with `auth_`.
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
