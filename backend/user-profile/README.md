# FootballCatch.UserProfile

Microservicio que gestiona el **perfil de usuario**: nickname, avatar, preferencias, privacidad.

---

## Eventos emitidos
- `profile.updated.v1`


## Eventos consumidos
- `identity.user.registered.v1`


---

## Infraestructura

- Base de datos propia (PostgreSQL).
- Endpoints REST expuestos según responsabilidades.
- Endpoint `/metrics` para Prometheus.
