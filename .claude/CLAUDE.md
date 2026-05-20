# CLAUDE.md — FootballCatch Project Context

Read this file at the start of every session before doing any work. It is the authoritative single-pager for the project. Longer details live in the linked docs — follow the links only when you need them.

---

## 1. Project Identity

**Name:** FootballCatch  
**What it is:** A football prediction platform built as a modular monolith, a web app, a mobile app, and a backoffice.  
**Stack:** .NET 10 backend, Next.js web + backoffice, React Native (Expo) mobile, PostgreSQL, Redis.  
**Deployment:** Ubuntu VPS, Docker Compose, GitHub Actions CI/CD.

---

## 2. Repository Layout

```
/backend/             # .NET 10 modular monolith (one folder per module)
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
  FootballCatch.sln   # Single solution at backend/ root with solution folders per module
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
/infra/               # docker-compose.yml, init.sql
```

Each backend module follows:

```
/src/
  {Module}.Domain/
  {Module}.Application/
  {Module}.Infrastructure/
  {Module}.Api/
  {Module}.Migrations/      # EF Core DbContext + all Migration classes
/tests/
  {Module}.UnitTests/             # Domain/application logic, no I/O
```

Backend integration tests are not per-module. They live in a single shared project at `backend/common/tests/FootballCatch.Common.IntegrationTests`, organised by cross-module scenario. See `docs/adr/0008-shared-integration-tests-project.md`.

All modules are referenced from a single `FootballCatch.sln` at `backend/` root with solution folders mirroring the physical module folders. There is no per-module `.sln` file.

---

## 3. Architecture

**Pattern:** DDD + CQRS per module. Layers: Domain → Application → Infrastructure → API.  
**Communication:**

- Sync → REST (via Nginx reverse proxy / BFF/Gateway)
- Async/decoupled → in-process events via the dispatcher pattern (see `backend/common/Messaging`)

**Key ADRs:**

- **Modular monolith** with an in-process event dispatcher. Cross-module integration events are routed in-process via `IEventPublisher<T : IIntegrationEvent>`. → `docs/adr/0006-modular-monolith-and-in-process-dispatcher.md`
- **Single PostgreSQL database, shared schema.** Bounded-context discipline is enforced in code at the module boundary. Tables are prefixed with the module short name (e.g. `catalog_teams`, `auth_users`) to avoid collisions. → `docs/adr/0007-shared-database-shared-schema.md`
- **EF Core Migrations** (Npgsql) for all DB migrations, applied automatically on startup. → `docs/adr/0005-use-efcore-migrations.md`

**Idempotency:** Handlers are invoked once per event in-process; if a handler can fail mid-transaction, design it to be retryable (deferred to follow-up).  
**Observability:** Serilog structured logs + correlation IDs, `/metrics` endpoint per module scraped by Prometheus, Grafana dashboards. Frontend monitoring via Sentry (web, backoffice, mobile).

---

## 4. Modules Catalogue

| Module             | Responsibility                                          | Publishes                                                                                                | Subscribes                                                                         |
| ------------------ | ------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| **auth**           | Registration, login, JWT, refresh tokens, device tokens | `identity.user.registered.v1`, `identity.user.disabled.v1`, `identity.deviceToken.added.v1`              | —                                                                                  |
| **user-profile**   | Nickname, avatar, preferences                           | `profile.updated.v1`                                                                                     | `identity.user.registered.v1`                                                      |
| **catalog**        | Competitions, teams                                     | `competition.created.v1`, `competition.updated.v1`, `team.upserted.v1`                                   | —                                                                                  |
| **fixtures**       | Match calendar, live scores, results                    | `match.upserted.v1`, `match.kickoff.v1`, `match.scoreChanged.v1`, `match.finalized.v1`                   | `competition.created.v1`, `team.upserted.v1`                                       |
| **predictions**    | User match predictions                                  | `prediction.submitted.v1`, `prediction.locked.v1`, `prediction.replaced.v1`                              | `match.upserted.v1`, `match.kickoff.v1`                                            |
| **scoring-engine** | Point calculation                                       | `scoring.userScoreUpdated.v1`, `scoring.matchScored.v1`                                                  | `prediction.submitted.v1`, `match.scoreChanged.v1`, `match.finalized.v1`           |
| **leagues**        | Private/public leagues, standings                       | `league.created.v1`, `league.joined.v1`, `league.left.v1`, `league.updated.v1`, `league.tableUpdated.v1` | `profile.updated.v1`, `scoring.userScoreUpdated.v1`, `identity.user.registered.v1` |
| **stats**          | Global leaderboard, KPIs                                | `leaderboard.updated.v1`, `leaderboard.kpi.updated.v1`                                                   | `scoring.userScoreUpdated.v1`, `league.tableUpdated.v1`                            |
| **notifications**  | Push + email                                            | `notification.sent.v1`                                                                                   | `match.kickoff.v1`, `prediction.locked.v1`, `scoring.userScoreUpdated.v1`          |

Module contracts live in `backend/common` — kept versioned (`.v1`, `.v2`, …) for forward compatibility even though events are in-process. **Never change a published event's shape — add a new version.**

---

## 5. Frontend Apps

| App                   | Tech                     | Key responsibilities                                                                                                                                                    |
| --------------------- | ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `frontend/web`        | Next.js 14+ (App Router) | Predictions, results, rankings, profile, private leagues. SSR/ISR for public pages.                                                                                     |
| `frontend/backoffice` | Next.js 14+ (App Router) | Competitions & team management, scoring rules config, user moderation.                                                                                                  |
| `frontend/app`        | React Native (Expo)      | Predictions, push notifications, rankings, profile.                                                                                                                     |
| `frontend/common`     | TypeScript npm package   | Shared domain + application + infrastructure. No React/RN deps in domain/application layers. Single entry point `src/index.ts`. SemVer — breaking changes = major bump. |

All three apps consume `@footballcatch/common`.

---

## 6. Three Main Actors (see `docs/use-cases.md` for full detail)

| Actor       | Channels            | Summary                                                                                                                    |
| ----------- | ------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| **User**    | Web app, mobile app | Register/login, submit & manage predictions, view results & rankings, join leagues, receive notifications                  |
| **Admin**   | Backoffice          | Manage competitions/teams/scoring rules, moderate users, monitor system health, manual fixture overrides                   |
| **Updater** | Background process  | Fetch from external football API → sync competitions, teams, fixtures, live scores, finalize results, trigger kickoff lock |

---

## 7. Code Conventions

### C# (.NET)

- PascalCase for classes, methods, properties; camelCase for locals and parameters.
- `record struct` for strongly-typed IDs.
- Test naming: `[UnitOfWork_StateUnderTest_ExpectedBehavior]` (NUnit).
- Full layered project structure per module: Domain / Application / Infrastructure / API.

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

Backend tests live in two kinds of project:

| Project                                  | Location                                          | Purpose                                                                                                | Key tools                        |
| ---------------------------------------- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------------ | -------------------------------- |
| `{Module}.UnitTests`                     | `backend/{module}/tests/`                         | Domain and application logic, no I/O                                                                   | NUnit, NSubstitute               |
| `FootballCatch.Common.IntegrationTests`  | `backend/common/tests/`                           | Single shared project. Real PostgreSQL via Testcontainers. Organised by cross-module scenario folder.  | NUnit, Testcontainers.PostgreSql |

The shared integration tests project references every module's `Application`, `Infrastructure`, and `Migrations` projects, plus `FootballCatch.Common`. A single `DatabaseFixture` starts one Testcontainers PostgreSQL per test run and applies every module's migrations against it before tests execute. Each module keeps its own `DbContext` and its own migrations history table (per ADR-0007). See ADR-0008 for the rationale and `docs/process/testing.md` for the full guide.

Guideline: tests that can be expressed with pure domain or application logic stay in `{Module}.UnitTests`. Only tests that need a real database, or that cross more than one module (typically via the in-process dispatcher), go in the shared integration project.

| Level               | Tool                                      | Requirement                                                                  |
| ------------------- | ----------------------------------------- | ---------------------------------------------------------------------------- |
| Unit (C#)           | NUnit + NSubstitute                       | ≥ 70% domain layer coverage                                                  |
| Integration (C#)    | NUnit + Testcontainers (PostgreSQL)       | Lives in the shared integration project; never mock the DB                   |
| Unit/Component (TS) | Jest + React Testing Library              | Component and hook logic                                                     |
| Contract            | CI check against Common Contracts package | Must pass before merge                                                       |
| E2E                 | Playwright/Cypress (web), Detox (mobile)  | Critical paths: submit prediction → scoring → leaderboard                    |

All tests run in GitHub Actions. **PRs cannot merge with failing tests.**

Full guide: `docs/process/testing.md`

---

## 9. Database Migrations (FluentMigrator)

- Location: `{Module}.Migrations` project under `src/` inside each module.
- Auto-applied on startup via `dbContext.Database.MigrateAsync()`.
- Migration name format: `{YYYYMMDDHHmm}_{PascalCaseDescription}` (e.g. `202401150930_CreatePredictionsTable`), generated by `dotnet ef migrations add`.

All modules' `*.Migrations` projects target the same PostgreSQL database (shared schema). To avoid table-name collisions across modules, prefix table names with the module short name (e.g. `catalog_teams`, `auth_users`, `predictions_predictions`). Each module retains its own `DbContext` and its own migration history table.

**Hard rules:**

- Never delete or modify an already-applied migration — add a new one.
- One logical change per migration.
- No business logic in migrations — schema changes only.
- Keep EF entity configurations in Infrastructure in sync with FluentMigrator migrations.

Full guide: `docs/process/migrations.md`

---

## 10. Git Workflow

**Model:** Simplified GitFlow.

| Branch      | Purpose                                         |
| ----------- | ----------------------------------------------- |
| `main`      | Always production-ready, tagged with SemVer     |
| `develop`   | Integration branch                              |
| `feature/*` | New features, branched from `develop`           |
| `release/*` | Pre-release stabilisation                       |
| `hotfix/*`  | Critical production fixes, branched from `main` |

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
# Start base infrastructure (PostgreSQL, Redis, Prometheus, Grafana)
docker compose -f infra/docker-compose.yml up -d

# Start all services
docker compose up -d
```

Local infrastructure defaults:

- PostgreSQL: `localhost:5432` | user: `testuser` | pass: `testpass!`

---

## 13. Agents

Specialised agent definitions live in `agents/`. Load the relevant agent when the task matches its scope.

| Agent                      | File                               | When to use                                                                                                                                                                                    |
| -------------------------- | ---------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **backend-engineer**       | `agents/backend_engineer.md`       | Any backend work: module implementation, domain logic, application layer, infrastructure, REST endpoints, event contracts, DB migrations, backend tests                                        |
| **technical-documenter**   | `agents/technical_documenter.md`   | Any documentation work: ADRs, technical specs, product decisions, process guides, service READMEs, API references, gotchas, runbooks, user manuals, onboarding material                        |
| **frontend-engineer**      | `agents/frontend_engineer.md`      | Any work on `frontend/common` (`@footballcatch/common`): domain entities, value objects, ports, use cases, HTTP clients, storage adapters, package config, build setup, tests                  |
| **nextjs-developer**       | `agents/nextjs_developer.md`       | Any work on `frontend/web` or `frontend/backoffice`: pages, layouts, server/client components, API routes, forms, responsive Tailwind UI, Context API state, consuming `@footballcatch/common` |
| **react-native-developer** | `agents/react_native_developer.md` | Any work on `frontend/app`: screens, Expo Router navigation, React Native Paper UI, push notifications, Context API state, EAS Build, App Store/Play Store compliance                          |

---

## 14. Key Documents Reference

| Topic                                           | Path                                                          |
| ----------------------------------------------- | ------------------------------------------------------------- |
| Use cases (User / Admin / Updater)              | `docs/use-cases.md`                                           |
| Architecture overview                           | `docs/architecture/architecture.md`                           |
| Modules map + events                            | `docs/architecture/microservices.md`                          |
| Infra & deployment                              | `docs/architecture/infra.md`                                  |
| Observability                                   | `docs/architecture/observability.md`                          |
| Code style                                      | `docs/process/code-style.md`                                  |
| Commit conventions                              | `docs/process/commits.md`                                     |
| GitFlow                                         | `docs/process/gitflow.md`                                     |
| Testing strategy                                | `docs/process/testing.md`                                     |
| DB migrations guide                             | `docs/process/migrations.md`                                  |
| Definition of Ready/Done                        | `docs/process/dod-dor.md`                                     |
| ADR: Superseded — was: RabbitMQ                 | `docs/adr/0001-use-rabbitmq.md`                               |
| ADR: Superseded — was: DB per service           | `docs/adr/0002-db-per-microservice.md`                        |
| ADR: FluentMigrator (superseded)                | `docs/adr/0003-use-fluentmigrator.md`                         |
| ADR: Sentry frontend monitoring                 | `docs/adr/0004-use-sentry-for-frontend-monitoring.md`         |
| ADR: EF Core Migrations                         | `docs/adr/0005-use-efcore-migrations.md`                      |
| ADR: Modular monolith and in-process dispatcher | `docs/adr/0006-modular-monolith-and-in-process-dispatcher.md` |
| ADR: Shared database, shared schema             | `docs/adr/0007-shared-database-shared-schema.md`              |
| ADR: Single shared integration tests project    | `docs/adr/0008-shared-integration-tests-project.md`           |
