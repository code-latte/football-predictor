# CLAUDE.md — FootballCatch Project Context

Read this file at the start of every session before doing any work. It is the authoritative single-pager for the project. Longer details live in the linked docs — follow the links only when you need them.

---

## 1. Project Identity

**Name:** FootballCatch  
**What it is:** A football prediction platform built with microservices, a web app, a mobile app, and a backoffice.  
**Stack:** .NET 10 backend, Next.js web + backoffice, React Native (Expo) mobile, PostgreSQL, RabbitMQ, Redis.  
**Deployment:** Ubuntu VPS, Docker Compose, GitHub Actions CI/CD.

---

## 2. Repository Layout

```
/backend/             # .NET 10 microservices (one folder per service)
  auth/
  catalog/
  common/             # Shared contracts, strongly-typed IDs, cross-cutting utilities
  fixtures/
  leagues/
  notifications/
  predictions/
  scoring-engine/
  stats/
  user-profile/
/frontend/
  app/                # React Native (Expo) — mobile
  backoffice/         # Next.js — admin panel
  common/             # npm package: @footballcatch/common (clean architecture, no React deps)
  web/                # Next.js — public web app (SSR + ISR)
/docs/
  architecture/       # architecture.md, microservices.md, infra.md, observability.md
  adr/                # Architecture Decision Records
  process/            # code-style.md, commits.md, gitflow.md, testing.md, migrations.md, dod-dor.md
  use-cases.md        # Full use-case catalogue (User / Admin / Updater actors)
/infra/               # docker-compose.yml, init.sql, rabbit-definitions.json
```

Each backend microservice follows:
```
/src/
  {Service}.Domain/
  {Service}.Application/
  {Service}.Infrastructure/
  {Service}.Api/
  {Service}.Migrations/      # EF Core DbContext + all Migration classes
/tests/
  {Service}.UnitTests/             # Domain/application logic, no I/O
  {Service}.IntegrationTests/      # Real PostgreSQL via Testcontainers
  {Service}.Tests.Infrastructure/  # Shared DB setup, fakes, builders
*.sln      # Solution file at service root
```

---

## 3. Architecture

**Pattern:** DDD + CQRS per service. Layers: Domain → Application → Infrastructure → API.  
**Communication:**
- Sync → REST (via Nginx reverse proxy / BFF/Gateway)
- Async → RabbitMQ events (fanout/topic exchanges)

**Key ADRs:**
- Each service has its **own PostgreSQL database** — no shared schemas. Cross-service data is replicated via events (projections). → `docs/adr/0002-db-per-microservice.md`
- **RabbitMQ** is the event bus. Kafka is a future option. → `docs/adr/0001-use-rabbitmq.md`
- **EF Core Migrations** (Npgsql) for all DB migrations, applied automatically on startup. → `docs/adr/0005-use-efcore-migrations.md`

**Idempotency:** Every event consumer applies the Inbox pattern.  
**Observability:** Serilog structured logs + correlation IDs, `/metrics` endpoint per service scraped by Prometheus, Grafana dashboards. Frontend monitoring via Sentry (web, backoffice, mobile).

---

## 4. Microservices Catalogue

| Service | Responsibility | Emits | Consumes |
|---|---|---|---|
| **auth** | Registration, login, JWT, refresh tokens, device tokens | `identity.user.registered.v1`, `identity.user.disabled.v1`, `identity.deviceToken.added.v1` | — |
| **user-profile** | Nickname, avatar, preferences | `profile.updated.v1` | `identity.user.registered.v1` |
| **catalog** | Competitions, teams | `competition.created.v1`, `competition.updated.v1`, `team.upserted.v1` | — |
| **fixtures** | Match calendar, live scores, results | `match.upserted.v1`, `match.kickoff.v1`, `match.scoreChanged.v1`, `match.finalized.v1` | `competition.created.v1`, `team.upserted.v1` |
| **predictions** | User match predictions | `prediction.submitted.v1`, `prediction.locked.v1`, `prediction.replaced.v1` | `match.upserted.v1`, `match.kickoff.v1` |
| **scoring-engine** | Point calculation | `scoring.userScoreUpdated.v1`, `scoring.matchScored.v1` | `prediction.submitted.v1`, `match.scoreChanged.v1`, `match.finalized.v1` |
| **leagues** | Private/public leagues, standings | `league.created.v1`, `league.joined.v1`, `league.left.v1`, `league.updated.v1`, `league.tableUpdated.v1` | `profile.updated.v1`, `scoring.userScoreUpdated.v1`, `identity.user.registered.v1` |
| **stats** | Global leaderboard, KPIs | `leaderboard.updated.v1`, `leaderboard.kpi.updated.v1` | `scoring.userScoreUpdated.v1`, `league.tableUpdated.v1` |
| **notifications** | Push + email | `notification.sent.v1` | `match.kickoff.v1`, `prediction.locked.v1`, `scoring.userScoreUpdated.v1` |

Integration event contracts live in `backend/common`. Events are versioned (`.v1`, `.v2`, …). **Never change a published event's shape — add a new version.**

---

## 5. Frontend Apps

| App | Tech | Key responsibilities |
|---|---|---|
| `frontend/web` | Next.js 14+ (App Router) | Predictions, results, rankings, profile, private leagues. SSR/ISR for public pages. |
| `frontend/backoffice` | Next.js 14+ (App Router) | Competitions & team management, scoring rules config, user moderation. |
| `frontend/app` | React Native (Expo) | Predictions, push notifications, rankings, profile. |
| `frontend/common` | TypeScript npm package | Shared domain + application + infrastructure. No React/RN deps in domain/application layers. Single entry point `src/index.ts`. SemVer — breaking changes = major bump. |

All three apps consume `@footballcatch/common`.

---

## 6. Three Main Actors (see `docs/use-cases.md` for full detail)

| Actor | Channels | Summary |
|---|---|---|
| **User** | Web app, mobile app | Register/login, submit & manage predictions, view results & rankings, join leagues, receive notifications |
| **Admin** | Backoffice | Manage competitions/teams/scoring rules, moderate users, monitor system health, manual fixture overrides |
| **Updater** | Background process | Fetch from external football API → sync competitions, teams, fixtures, live scores, finalize results, trigger kickoff lock |

---

## 7. Code Conventions

### C# (.NET)
- PascalCase for classes, methods, properties; camelCase for locals and parameters.
- `record struct` for strongly-typed IDs.
- Test naming: `[UnitOfWork_StateUnderTest_ExpectedBehavior]` (NUnit).
- Full layered project structure per service: Domain / Application / Infrastructure / API.

### TypeScript — Web & Backoffice (Next.js)
- ESLint + Prettier enforced.
- App Router (`app/` convention). Server Components by default; `'use client'` only when strictly necessary.
- Functional components and hooks only (`useSomething` over HOCs).
- Folder-by-feature inside `app/` and `components/`.

### TypeScript — Mobile (React Native / Expo)
- ESLint + Prettier enforced.
- Expo managed workflow.
- Functional components and hooks only.
- Folder-by-feature inside `screens/` and `components/`.

### TypeScript — `@footballcatch/common`
- Zero React/RN dependencies in domain and application layers.
- Infrastructure layer may use fetch/axios and platform-agnostic interfaces.
- All exports through `src/index.ts`.

---

## 8. Testing Strategy

Each backend microservice has three test projects under `tests/`:

| Project | Purpose | Key tools |
|---|---|---|
| `{Service}.UnitTests` | Domain and application logic, no I/O | NUnit, NSubstitute |
| `{Service}.IntegrationTests` | Real PostgreSQL via Testcontainers | NUnit, Testcontainers.PostgreSql |
| `{Service}.Tests.Infrastructure` | Shared DB setup (`DatabaseFixture`), fakes, object-mother builders — no test classes | Testcontainers.PostgreSql, NSubstitute |

`{Service}.Tests.Infrastructure` references `{Service}.Migrations` and calls `dbContext.Database.MigrateAsync()` to build the schema from scratch against the containerised database before integration tests run.

| Level | Tool | Requirement |
|---|---|---|
| Unit (C#) | NUnit + NSubstitute | ≥ 70% domain layer coverage |
| Integration (C#) | NUnit + Testcontainers (PostgreSQL) | At least one integration test per service; never mock the DB |
| Unit/Component (TS) | Jest + React Testing Library | Component and hook logic |
| Contract | CI check against Common Contracts package | Must pass before merge |
| E2E | Playwright/Cypress (web), Detox (mobile) | Critical paths: submit prediction → scoring → leaderboard |

All tests run in GitHub Actions. **PRs cannot merge with failing tests.**

Full guide: `docs/process/testing.md`

---

## 9. Database Migrations (EF Core)

- Location: `{Service}.Migrations` project under `src/` inside each service.
- Auto-applied on startup via `dbContext.Database.MigrateAsync()`.
- Migration name format: `{YYYYMMDDHHmm}_{PascalCaseDescription}` (e.g. `202401150930_CreatePredictionsTable`), generated by `dotnet ef migrations add`.

**Hard rules:**
- Never delete or modify an already-applied migration — add a new one.
- One logical change per migration.
- No business logic in migrations — schema changes only.
- Always review the generated migration file before committing.
- Commit the `DbContextModelSnapshot` alongside its migration.

Full guide: `docs/process/migrations.md`

---

## 10. Git Workflow

**Model:** Simplified GitFlow.

| Branch | Purpose |
|---|---|
| `main` | Always production-ready, tagged with SemVer |
| `develop` | Integration branch |
| `feature/*` | New features, branched from `develop` |
| `release/*` | Pre-release stabilisation |
| `hotfix/*` | Critical production fixes, branched from `main` |

**Commit format:** Conventional Commits (optionally with Gitmoji)
```
<type>(optional scope): <description>

Types: feat | fix | refactor | test | docs | chore
```

Examples: `feat: add endpoint to submit predictions`, `fix: correct score on penalty shootout`

---

## 11. Definition of Done

A task is **done** when:
1. Code is reviewed (PR or pair programming).
2. Unit tests written and passing.
3. Integration/contract tests updated.
4. Logging and metrics in place (Serilog + `/metrics`).
5. Relevant documentation updated.
6. Deployed to `main` and production-ready.

---

## 12. Local Development

```bash
# Start base infrastructure (RabbitMQ, PostgreSQL, Redis, Prometheus, Grafana)
docker compose -f infra/docker-compose.yml up -d

# Start all services
docker compose up -d
```

Local infrastructure defaults:
- RabbitMQ AMQP: `localhost:5672` | Management UI: `localhost:15672`
- PostgreSQL: `localhost:5432` | user: `testuser` | pass: `testpass!`

---

## 13. Agents

Specialised agent definitions live in `agents/`. Load the relevant agent when the task matches its scope.

| Agent | File | When to use |
|---|---|---|
| **backend-engineer** | `agents/backend_engineer.md` | Any backend work: microservice implementation, domain logic, application layer, infrastructure, REST endpoints, event contracts, DB migrations, backend tests |
| **technical-documenter** | `agents/technical_documenter.md` | Any documentation work: ADRs, technical specs, product decisions, process guides, service READMEs, API references, gotchas, runbooks, user manuals, onboarding material |
| **frontend-engineer** | `agents/frontend_engineer.md` | Any work on `frontend/common` (`@footballcatch/common`): domain entities, value objects, ports, use cases, HTTP clients, storage adapters, package config, build setup, tests |
| **nextjs-developer** | `agents/nextjs_developer.md` | Any work on `frontend/web` or `frontend/backoffice`: pages, layouts, server/client components, API routes, forms, responsive Tailwind UI, Context API state, consuming `@footballcatch/common` |
| **react-native-developer** | `agents/react_native_developer.md` | Any work on `frontend/app`: screens, Expo Router navigation, React Native Paper UI, push notifications, Context API state, EAS Build, App Store/Play Store compliance |

---

## 14. Key Documents Reference

| Topic | Path |
|---|---|
| Use cases (User / Admin / Updater) | `docs/use-cases.md` |
| Architecture overview | `docs/architecture/architecture.md` |
| Microservices map + events | `docs/architecture/microservices.md` |
| Infra & deployment | `docs/architecture/infra.md` |
| Observability | `docs/architecture/observability.md` |
| Code style | `docs/process/code-style.md` |
| Commit conventions | `docs/process/commits.md` |
| GitFlow | `docs/process/gitflow.md` |
| Testing strategy | `docs/process/testing.md` |
| DB migrations guide | `docs/process/migrations.md` |
| Definition of Ready/Done | `docs/process/dod-dor.md` |
| ADR: RabbitMQ | `docs/adr/0001-use-rabbitmq.md` |
| ADR: DB per service | `docs/adr/0002-db-per-microservice.md` |
| ADR: FluentMigrator (superseded) | `docs/adr/0003-use-fluentmigrator.md` |
| ADR: Sentry frontend monitoring | `docs/adr/0004-use-sentry-for-frontend-monitoring.md` |
| ADR: EF Core Migrations | `docs/adr/0005-use-efcore-migrations.md` |
