# Observability

## Logging
- **Serilog** structured logging.  
- Correlation IDs for tracing across services.  
- Log levels: Info, Warning, Error.  

## Metrics
- Each service exposes `/metrics` endpoint.  
- Default metrics: HTTP requests, DB queries, RabbitMQ consumers.  
- Custom metrics: domain-specific (predictions submitted, leagues created).  

## Tracing
- Correlation IDs propagated through HTTP headers and RabbitMQ message properties.  
- Optional OpenTelemetry integration in the future.  

## Dashboards
- Grafana dashboards for:  
  - Service health.  
  - Requests/sec.  
  - Event throughput.  
  - DB performance.  

## Frontend Monitoring

- **Tool:** Sentry
- **Apps covered:** `frontend/web`, `frontend/backoffice`, `frontend/app`
- **SDKs:**
  - `@sentry/nextjs` — used by `frontend/web` and `frontend/backoffice`
  - `@sentry/react-native` — used by `frontend/app` (Expo managed workflow)
- **What is captured:**
  - Unhandled errors and unhandled promise rejections (all three apps)
  - Native crashes (mobile)
  - Web Vitals: LCP, FID, CLS (web and backoffice)
  - App start-up time and slow transactions (mobile)
- **Configuration:**
  - `NEXT_PUBLIC_SENTRY_DSN` environment variable — `frontend/web` and `frontend/backoffice`
  - `SENTRY_DSN` environment variable — `frontend/app`
  - Source maps and debug symbols are uploaded to Sentry during the CI/CD production build step.
- **PII policy:** User ID may be attached to Sentry events to aid debugging. Passwords, tokens, and email addresses must be scrubbed via the `beforeSend` hook before any event is transmitted. No other personally identifiable information may be sent.

See `docs/adr/0004-use-sentry-for-frontend-monitoring.md` for the decision rationale and full list of obligations.

---
