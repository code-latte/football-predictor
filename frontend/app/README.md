# FootballCatch App

Aplicación móvil en **React Native** (Expo).

---

## Responsabilidades

- Envío y consulta de predicciones
- Recepción de notificaciones push
- Rankings y perfil de usuario en móvil

---

## Tecnologías

- React Native + Expo
- TypeScript
- Consume `@footballcatch/common` para lógica de dominio y acceso a APIs

---

## Estructura

```
/src/
  screens/    # Pantallas de la app
  components/ # Componentes reutilizables
  hooks/      # Custom hooks
/tests/
```

---

## Monitoring

- **Sentry** (`@sentry/react-native`) for crash reporting and error monitoring.
- Captures: unhandled errors, native crashes, unhandled promise rejections, app start performance.
- Configuration: set `SENTRY_DSN` environment variable.
- Source maps and debug symbols uploaded to Sentry on each production build (via CI/CD).
- PII policy: user ID may be attached; passwords, tokens, and email addresses must be scrubbed via `beforeSend`.
