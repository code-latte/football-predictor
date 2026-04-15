# ADR 0004: Use Sentry for Frontend Error Monitoring

## Status
Accepted

## Context

The three frontend applications — `frontend/web` (Next.js), `frontend/backoffice` (Next.js), and `frontend/app` (React Native/Expo) — had no error monitoring or crash reporting in place. Unhandled errors, unhandled promise rejections, and mobile crashes were invisible in production, making it impossible to detect regressions, diagnose user-reported issues, or measure frontend reliability.

Backend observability is covered by Serilog structured logs, Prometheus metrics, and Grafana dashboards (see `docs/architecture/observability.md`). No equivalent existed on the frontend.

The following alternatives were considered:

| Tool | Reason rejected |
|---|---|
| **Datadog RUM** | Significantly higher cost; overkill for early stage; Datadog is not used elsewhere in the stack |
| **LogRocket** | Session replay is valuable but the pricing model is session-based, which becomes expensive at scale; no first-class Expo SDK |
| **Rollbar** | Mature tool but no meaningful advantage over Sentry for this stack; smaller community for Next.js and React Native |
| **Self-hosted error tracking (e.g. GlitchTip)** | Adds infrastructure maintenance burden with no cost benefit at early stage; source map management is more complex |

## Decision

Use **Sentry** for error monitoring and crash reporting across all three frontend applications.

- `frontend/web` and `frontend/backoffice` use the `@sentry/nextjs` SDK.
- `frontend/app` uses the `@sentry/react-native` SDK, which has first-class Expo support.

Sentry was chosen because:
- It provides official, well-maintained SDKs for Next.js and React Native/Expo — both frameworks used in this project.
- A single Sentry organisation covers all three apps, giving unified error visibility across web, backoffice, and mobile.
- The free tier is sufficient for the early stage of the project.
- It integrates with GitHub for source map uploads, commit tracking, and release tagging via CI/CD.
- Performance monitoring (Web Vitals for Next.js, app start-up time for React Native) is available at no additional configuration cost.

Each application is configured as a separate Sentry **project** within the same organisation to maintain event isolation and separate alerting rules per app.

## Consequences

**Positive:**
- Unhandled errors, unhandled promise rejections, and mobile crashes are captured with full stack traces in production.
- Web Vitals (LCP, FID, CLS) are tracked automatically for `frontend/web` and `frontend/backoffice`.
- App start-up time and slow transactions are tracked for `frontend/app`.
- Source maps are uploaded during CI/CD builds, enabling readable stack traces from minified production bundles.
- GitHub integration allows Sentry to link errors to the commit that introduced them and auto-resolve issues when a fix is deployed.

**Obligations created:**
- Each application requires a Sentry DSN configured as an environment variable:
  - `NEXT_PUBLIC_SENTRY_DSN` for `frontend/web` and `frontend/backoffice`.
  - `SENTRY_DSN` for `frontend/app`.
- Source maps must be uploaded as part of every production build and deployment in CI/CD.
- **PII scrubbing is mandatory.** The `beforeSend` hook must be configured in each app to strip passwords, tokens, and email addresses from error payloads before they are sent to Sentry. User ID may be attached to events for debugging purposes, but no other personally identifiable information may be sent.
- Sentry credentials (DSN values, auth tokens for source map upload) must be stored as GitHub Actions secrets and never committed to the repository.
- A Sentry release must be created and associated with the deployed commit SHA on each production deployment to enable release tracking and regression detection.
