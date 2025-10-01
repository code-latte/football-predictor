# football-predictor

Plataforma de predicciones de fútbol basada en microservicios, con aplicaciones móviles y web.  
Este repositorio contiene **todo el ecosistema**:

- **Backend**

  - Microservicios en .NET 8 con arquitectura limpia (DDD + CQRS).
  - RabbitMQ como bus de eventos.
  - PostgreSQL por microservicio (una BD por bounded context).
  - Redis para caché y locks.
  - Observabilidad con Prometheus, Grafana y logs estructurados con Serilog.

- **Front-web**

  - Aplicación React para acceso web

- **Front-app**

  - Aplicación móvil en React Native

- **Front-backoffice**
  - Aplicación web en React para administración: gestión de competiciones, reglas, moderación.

---

## Estructura del repo

```
/backend/                # Código de microservicios .NET
/frontend-web/           # React web
/frontend-app/           # React Native
/frontend-backoffice/    # React admin
/docs/                   # Documentación de arquitectura, ADRs, etc.
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
