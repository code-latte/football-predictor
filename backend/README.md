# Backend de FootballCatch

Aquí viven los microservicios .NET que forman el backend del sistema.

---

## Convenciones de proyecto

Cada microservicio debe cumplir:

- **Estructura de carpetas**

  ```
  /src/      # Código del microservicio
  /tests/    # Tests unitarios e integración
  *.sln      # Solution en la raíz del microservicio
  ```

- **Tecnologías obligatorias**

  - **.NET 8**
  - **NUnit** para testing
  - **Serilog** para logging estructurado
  - **Prometheus** para métricas (endpoint `/metrics`)
  - **Grafana** para visualización (config centralizada en `/docs/infra/`)

- **Arquitectura**

  - Capas: Domain / Application / Infrastructure / API
  - DDD + CQRS: entidades, value objects, agregados.
  - Eventos de dominio → convertidos en eventos de integración para RabbitMQ.

- **Tests**
  - Unit tests (mínimo 70% coverage en dominio).
  - Contract tests (para eventos de integración).
  - Test de integración mínima con DB (NUnit + Testcontainers opcional).

---

## Microservicios actuales

- `auth` → Registro, login, auth (JWT).
- `user-profile` → Nicknames, avatares.
- `catalog` → Info de equipos, ligas, jugadores.
- `fixtures` → Calendario y resultados de partidos.
- `predictions` → Gestión de predicciones de usuarios.
- `scoring-engine` → Cálculo de puntos y totales.
- `leagues` → Ligas privadas y rankings.
- `notifications` → Push y correos.
- `stats` → Tablas globales y estadísticas.

---

## Guías adicionales

- **Eventos de integración**: están versionados (`.v1`, `.v2` …) y definidos en `Contracts` de cada MS.
- **Idempotencia**: cada consumidor aplica patrón _Inbox_.
- **Persistencia**: PostgreSQL por servicio.
