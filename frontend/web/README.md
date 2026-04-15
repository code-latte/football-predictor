# FootballCatch Web

**Next.js** application for web access to the predictions platform.

---

## Responsibilities

- Viewing predictions, results, and rankings
- User profile and private leagues
- SSR/ISR for public pages (SEO)

---

## Technologies

- Next.js 14+ (App Router)
- TypeScript
- Consumes `@footballcatch/common` for domain logic and API access

---

## Structure

```
/src/
  app/        # Routes (App Router)
  components/ # Reusable UI components
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
