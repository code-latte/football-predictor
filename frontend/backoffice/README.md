# FootballCatch Backoffice

Panel de administración en **Next.js** para gestión interna de la plataforma.

---

## Responsabilidades

- Gestión de competiciones y equipos
- Configuración de reglas de puntuación
- Moderación de usuarios y ligas

---

## Tecnologías

- Next.js 14+ (App Router)
- TypeScript
- Consume `@footballcatch/common` para lógica de dominio y acceso a APIs

---

## Estructura

```
/src/
  app/        # Rutas (App Router)
  components/ # Componentes UI reutilizables
  hooks/      # Custom hooks
/tests/
```

---

## Monitoring

- **Sentry** (`@sentry/nextjs`) for error and performance monitoring.
- Captures: unhandled errors, unhandled promise rejections, Web Vitals.
- Configuration: set `NEXT_PUBLIC_SENTRY_DSN` environment variable.
- Source maps uploaded to Sentry on each production build (via CI/CD).
- PII policy: user ID may be attached; passwords, tokens, and email addresses must be scrubbed via `beforeSend`.
