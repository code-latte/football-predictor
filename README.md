# football-predictor

Plataforma de predicciones de fútbol basada en microservicios, con aplicaciones móviles y web.  
Este repositorio contiene **todo el ecosistema**:

- **Backend**

  - Microservicios en .NET 8 con arquitectura limpia (DDD + CQRS).
  - RabbitMQ como bus de eventos.
  - PostgreSQL por microservicio (una BD por bounded context).
  - Redis para caché y locks.
  - Observabilidad con Prometheus, Grafana y logs estructurados con Serilog.

- **Frontend**

  - `frontend/web` → Next.js web app (SSR + ISR) para acceso web a predicciones y rankings.
  - `frontend/app` → Aplicación móvil en React Native (Expo).
  - `frontend/backoffice` → Next.js admin panel para gestión de competiciones, reglas y moderación.
  - `frontend/common` → Librería compartida con clean architecture (domain + application + infrastructure), publicada como npm package.

---

## Estructura del repo

```
/backend/           # Microservicios .NET
/frontend/
  web/              # Next.js web
  app/              # React Native
  backoffice/       # Next.js backoffice
  common/           # npm package compartido
/docs/              # Documentación de arquitectura, ADRs, etc.
/infra/             # Docker, Postgres, RabbitMQ
```

---

## Arquitectura en resumen

- Cada microservicio es **autónomo**: tiene su propio repo lógico dentro de `/backend/` con su BD y contratos de eventos.
- La comunicación es:
  - **Sincronía** → llamadas REST al BFF/Gateway.
  - **Asincronía** → eventos en RabbitMQ (fanout/topic).
- CI/CD con **GitHub Actions**: build, test y despliegue a VPS con Docker Compose.

---

## Cómo levantar todo en local

```bash
# Arrancar infra base
docker compose -f infra/docker-compose.yml up -d

# Levantar todos los servicios
docker compose up -d
```

Infra base incluye:

- RabbitMQ
- PostgreSQL
- Redis
- Prometheus + Grafana
