# FootballCatch Web

Aplicación **Next.js** para acceso web a la plataforma de predicciones.

---

## Responsabilidades

- Visualización de predicciones, resultados y rankings
- Perfil de usuario y ligas privadas
- SSR/ISR para páginas públicas (SEO)

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
