# ADR 0010: Use Microsoft.Extensions.Http.Resilience for HTTP Retry

## Status
Proposed

## Context

The Updater calls an external football data provider (see ADR-0009) and, separately, the Catalog and Fixtures REST APIs. Network calls fail transiently: DNS hiccups, TLS resets, provider-side 5xx during deploys, request-timeout responses. Without a retry policy, those transient failures bubble up as fatal job errors and require manual reruns.

We need a standard, opinionated retry mechanism that:

- Plugs into the .NET `HttpClient` pipeline so every outbound call gets the same treatment without per-call boilerplate.
- Distinguishes transient failures (retry) from business failures (do not retry, surface to caller).
- Uses exponential backoff with jitter to avoid synchronised retry storms.
- Is configurable per-environment (retry count, base delay) via the options pattern.

The first piece of infrastructure to need this is the typed `ApiFootballClient`; subsequent typed clients (Catalog, Fixtures) will share the same approach, so the choice is structural rather than local.

## Decision

Outbound `HttpClient` retry is implemented with **`Microsoft.Extensions.Http.Resilience` (Polly v8)**, wired via `AddResilienceHandler` on each typed `HttpClient` registration.

The strategy applied in `AddUpdaterInfrastructure` is:

- **Retry** with:
  - `MaxRetryAttempts = options.RetryCount` (default `3`).
  - `Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds)` (default `1.0`).
  - `DelayBackoffType = Exponential`.
  - `UseJitter = true`.
- **`ShouldHandle`** triggers on:
  - `HttpRequestException` (transport-level failure).
  - HTTP status `>= 500` (server error).
  - HTTP status `408` (request timeout).

Retry parameters are sourced from `ApiFootballOptions` (validated at startup). The same shape will be reused by future typed clients via their own options classes.

## Consequences

**Positive:**
- One pipeline, one place to reason about retry semantics; per-call code stays clean.
- Exponential backoff + jitter is the right default for transient-failure handling at scale.
- `Microsoft.Extensions.Http.Resilience` is the current, supported library going forward; it integrates with the standard resilience telemetry and health-check primitives we are likely to want as observability matures.

**Negative / obligations:**
- Pins us to Polly v8's API surface. Upgrading Polly across major versions will require touching every `AddResilienceHandler` registration.
- The retry policy intentionally does **not** handle business 4xx (`401`, `403`, `404`, `429`). Callers must handle those explicitly — `429` in particular needs job-level back-off, since the HTTP handler will surface it unchanged.
- Misconfigured `RetryCount` × `BaseDelaySeconds` can amplify load on a struggling upstream. The defaults (3 retries, 1 s base, exponential, jittered) are conservative; reviewers must keep them that way.

## Alternatives Considered

| Option | Reason rejected |
|---|---|
| **`Microsoft.Extensions.Http.Polly` (Polly v7 wrapper)** | The older `AddPolicyHandler` wrapper package. In maintenance mode; `Microsoft.Extensions.Http.Resilience` is its successor and is where new features (standard resilience handlers, built-in telemetry) land. Picking the legacy package for a brand-new client would be a knowing step backwards. |
| **Hand-rolled retry loop in each client** | Boilerplate per call site, easy to get the backoff math wrong (forgetting jitter, drifting between clients), and impossible to manage centrally as more clients are added. |
| **No retry — let jobs fail and rely on the next scheduled run** | Acceptable for jobs that genuinely run on a short cadence (e.g. live polling every 60 s), but unacceptable for daily syncs where a single transient blip would mean a 24-hour stale window. A baseline retry policy is cheaper and more predictable. |

## References
- ADR-0009 — Use api-sports.io as external football data provider.
- `backend/updater/README.md` — API client section.
- `Microsoft.Extensions.Http.Resilience` — official .NET resilience package built on Polly v8.
