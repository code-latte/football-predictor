# FootballCatch.Auth

Microservicio responsable de la **autenticación y autorización** de usuarios (registro, login, JWT, refresh tokens).

---

## Eventos emitidos
- `identity.user.registered.v1`
- `identity.user.disabled.v1`
- `identity.deviceToken.added.v1`


## Eventos consumidos
- Ninguno


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
