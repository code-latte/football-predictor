# ADR 0009: Use api-sports.io as External Football Data Provider

## Status
Proposed

## Context

The Updater module is responsible for keeping the platform's competitions, teams, fixtures, live scores, and results in sync with an external source of truth. It needs an external football data provider it can poll on a schedule.

`docs/use-cases.md` (UC-UP01..UC-UP06) explicitly leaves the provider undecided and lists two candidates that fit the platform's needs:

- **api-sports.io (API-Football)** — `https://v3.football.api-sports.io/`. Auth via the `x-apisports-key` request header.
- **football-data.org** — alternative provider with its own coverage / quota model.

Both providers cover the same primitives the Updater needs (competitions, teams, fixtures, live scoring, results). The first concrete piece of the Updater (`IApiFootballClient` + typed `HttpClient`) needs a chosen base URL, auth scheme, and configuration shape, so the choice cannot stay implicit.

## Decision

The Updater integrates with **api-sports.io's football API** at `https://v3.football.api-sports.io/`. Authentication is performed via the `x-apisports-key` HTTP header, sourced from `ApiFootballOptions.ApiKey`.

Provider details are isolated behind the Application-layer port `IApiFootballClient`. The concrete `ApiFootballClient` lives in `FootballCatch.Updater.Infrastructure` as a typed `HttpClient` and is the only place that knows the provider's wire format (base URL, header name, response schema).

## Consequences

**Positive:**
- Concrete, testable target for the Updater's first vertical slice; configuration and DI wiring stop being hypothetical.
- Auth scheme is dead simple (single header), no OAuth / token-refresh dance.
- Provider lock-in is mitigated by the port/adapter split: `IApiFootballClient` belongs to the Application layer, so switching providers becomes a re-implementation of one Infrastructure class (and probably a new options record), not a cross-cutting refactor.

**Negative / obligations:**
- Free-tier rate limits apply; see the provider's docs for current per-minute and per-day quotas. The Updater's job scheduling and `429` handling must respect them.
- Coupling to api-sports.io's JSON shape lives in Infrastructure. Any field rename or breaking change on the provider's side forces an Infrastructure update; the Application layer should remain untouched.
- The decision is **Proposed** rather than Accepted because stakeholder confirmation of the commercial tier / quota fit is still pending. If that conversation forces a switch to football-data.org, only Infrastructure should need to change.

## Alternatives Considered

| Option | Reason rejected / deferred |
|---|---|
| **football-data.org** | Viable alternative on coverage. Deferred — pending stakeholder confirmation on quota and pricing fit. The port/adapter split keeps this option reversible if api-sports.io proves unsuitable. |
| **Multiple providers behind a single port (fallback chain)** | Premature. Adds significant complexity (deduplication, conflict resolution, per-provider rate accounting) before we have evidence a single provider is insufficient. Revisit only if reliability or coverage of the chosen provider becomes a real problem. |

## References
- `docs/use-cases.md` — UC-UP01..UC-UP06 (Updater actor).
- `backend/updater/README.md` — API client section and `ApiFootball` configuration table.
- ADR-0010 — Use `Microsoft.Extensions.Http.Resilience` for HTTP retry.
