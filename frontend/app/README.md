# FootballCatch App

Mobile application in **React Native** (Expo).

---

## Responsibilities

- Submitting and querying predictions
- Receiving push notifications
- Rankings and user profile on mobile

---

## Technologies

- React Native + Expo
- TypeScript
- Consumes `@footballcatch/common` for domain logic and API access

---

## Structure

```
/src/
  screens/    # App screens
  components/ # Reusable components
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
