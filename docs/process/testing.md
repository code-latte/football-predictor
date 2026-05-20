# Testing Strategy

Backend tests are organised into two kinds of project:

- One **`{Module}.UnitTests`** per module, living at `backend/{module}/tests/`. Pure domain and application logic, no I/O.
- A **single shared integration tests project** at `backend/common/tests/FootballCatch.Common.IntegrationTests`. Every test that needs a real database — including any scenario that crosses module boundaries — lives here.

See ADR-0008 for the rationale behind consolidating integration tests into a single shared project.

---

## Test project structure

```
backend/
  {module}/
    tests/
      {Module}.UnitTests/                       ← Pure domain and application logic, no I/O
  common/
    tests/
      FootballCatch.Common.IntegrationTests/    ← Shared: real DB, cross-module scenarios
```

---

## `{Module}.UnitTests`

Tests domain entities, value objects, domain services, and application use cases in isolation. No database, no network, no file system.

**References:**
- `{Module}.Domain`
- `{Module}.Application`
- `FootballCatch.Common.IntegrationTests` is **not** referenced from unit test projects. Builders, object mothers, and fakes live alongside it; if any of them are useful to unit tests as well, promote them to a small shared helpers project — do not pull the integration project into unit tests.

**NuGet packages:**
```xml
<PackageReference Include="NUnit" Version="4.*" />
<PackageReference Include="NUnit3TestAdapter" Version="4.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
<PackageReference Include="NSubstitute" Version="5.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
```

**Conventions:**
- Test method naming: `[UnitOfWork_StateUnderTest_ExpectedBehavior]`.
- Use NSubstitute to substitute application-layer ports (e.g. `IMatchRepository`, `IEventPublisher`). Never substitute domain objects.
- Target ≥ 70% line coverage on the domain layer.

---

## `FootballCatch.Common.IntegrationTests`

The single shared backend integration test project. Every test that requires a real PostgreSQL database — whether it exercises one module's repositories or a cross-module flow through the in-process event dispatcher — lives here.

**Location:**
```
backend/common/tests/FootballCatch.Common.IntegrationTests/
```

**References:**
- Every module's `Application`, `Infrastructure`, and `Migrations` projects.
- `FootballCatch.Common`.

**NuGet packages:**
```xml
<PackageReference Include="NUnit" Version="4.*" />
<PackageReference Include="NUnit3TestAdapter" Version="4.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="NSubstitute" Version="5.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" />
```

### Folder layout

Tests are organised by **cross-module scenario**, not by module. Each scenario folder owns the end-to-end flow it describes, regardless of how many modules participate.

```
FootballCatch.Common.IntegrationTests/
  Scenarios/
    PredictionSubmission/      ← e.g. predictions → scoring-engine via prediction.submitted.v1
    UserRegistration/          ← e.g. auth → user-profile via identity.user.registered.v1
    MatchFinalization/         ← e.g. fixtures → scoring-engine via match.finalized.v1
    LeagueJoin/                ← e.g. leagues + profile + scoring updates
  Fixtures/                    ← DatabaseFixture and any related base test classes
  Builders/                    ← Object-mother / fluent builders shared across scenarios
  Fakes/                       ← In-memory fakes (e.g. FakeClock, FakeEmailSender)
```

Each scenario folder contains the tests that describe one cross-module flow plus any helpers that are specific to it. Helpers that are genuinely shared across scenarios live under `Builders/` or `Fakes/`.

### The `DatabaseFixture` pattern

The integration suite uses a single shared `DatabaseFixture` that:

1. **Manages a Testcontainers PostgreSQL container** for the test run. The container is started before any test executes and disposed when the run ends.
2. **Applies every module's migrations** against the container, in turn, to produce the same shared schema the application runs against in production (see ADR-0007). Each module retains its own migrations history table (`__{module}_migrations_history`).
3. **Exposes the connection string** to tests, so each test can construct the module `DbContext`(s) it needs against the same database.
4. Provides hooks for per-test cleanup or transactional isolation, so scenarios do not leak state into one another.

The fixture description above is intentionally tool-agnostic: it states what the fixture does, not which migration runner invokes the migrations. The implementation of the fixture is a follow-up task — it is not in place yet, and no integration tests can be written until it lands.

### Conventions

- **Never mock the database.** Use the real PostgreSQL container exposed by `DatabaseFixture`.
- Test classes consume the fixture through the standard NUnit fixture mechanism (e.g. a shared one-time setup that resolves the connection string from `DatabaseFixture`).
- **One scenario per test class** is preferred. A scenario test reads like a story: arrange the world, trigger the cross-module action, assert the side-effects everywhere they should appear.
- Cross-module assertions are first-class. A test that submits a prediction and then asserts on a row written by `scoring-engine` is exactly the kind of test this project exists for.

### What goes where

Use this project for any test that:
- Needs a real database, **or**
- Crosses more than one module (typically via the in-process event dispatcher).

Anything that can be expressed with pure domain or application logic — entity invariants, value-object behaviour, use-case orchestration against substituted ports — stays in `{Module}.UnitTests`. The shared integration project is not a dumping ground for tests that could have been unit tests; reviewers should push such tests back into the relevant module's unit project.

---

## Dependency graph summary

```
{Module}.UnitTests
  → {Module}.Domain
  → {Module}.Application

FootballCatch.Common.IntegrationTests
  → FootballCatch.Common
  → every module's {Module}.Application
  → every module's {Module}.Infrastructure
  → every module's {Module}.Migrations
```

---

## Frontend tests

| Level | Tool | Requirement |
|---|---|---|
| Unit | Jest + React Testing Library | Component and hook logic in isolation |
| E2E | Playwright or Cypress (web/backoffice), Detox (mobile) | Critical paths: submit prediction → scoring → leaderboard update |

---

## Contract tests

Integration events are validated against the contracts defined in `backend/common`. A CI check confirms that event shapes published and consumed by each module match the common contracts package. This check must pass before any PR is merged.

---

## Continuous integration

All tests run in GitHub Actions on every pull request. **A PR cannot be merged if any test fails.**

---

## Open follow-ups

- **`DatabaseFixture` implementation.** The shared integration tests project depends on a `DatabaseFixture` that manages the Testcontainers PostgreSQL lifecycle and applies every module's migrations against it. The fixture is not implemented yet; no integration tests can be authored until it lands. Tracked alongside the work that introduced this project (ADR-0008).
- **Migration runner mismatch (ADR-0005 vs. current code).** ADR-0005 records the decision to use EF Core Migrations for all backend modules. The catalog module's current migrations (e.g. `backend/catalog/src/FootballCatch.Catalog.Migrations/Migrations/202605020900_CreateCompetitionsTable.cs`) are written against FluentMigrator. This document deliberately describes the integration test setup in tool-agnostic terms ("applies every module's migrations") and **does not resolve the conflict here**. A separate decision is required to either re-affirm ADR-0005 and migrate the catalog code to EF Core, or supersede ADR-0005 in favour of FluentMigrator. The migrations guide (`docs/process/migrations.md`) and `CLAUDE.md` will need to be aligned once that decision is made.
