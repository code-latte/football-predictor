# ADR 0008: Single Shared Integration Tests Project

## Status
Accepted

## Context

ADR-0006 collapsed the originally planned nine microservices into a modular monolith with cross-module communication routed in-process via the event dispatcher. ADR-0007 collapsed the database-per-service rule into a single PostgreSQL database with a shared schema, with module boundaries enforced in code rather than at the data layer.

The per-module integration test layout — one `{Module}.IntegrationTests` and one `{Module}.Tests.Infrastructure` project per module — was carried over from the microservices era. It assumed each module had its own database and that integration verification meant exercising one module's repositories against its own datastore. After ADR-0006 and ADR-0007, neither assumption holds:

- The database is shared. A per-module fixture that spins up its own PostgreSQL container and applies only that module's migrations cannot verify any behaviour that crosses a module boundary.
- The meaningful integration boundary in this architecture is now the **in-process event dispatcher**: e.g. `predictions` publishing `prediction.submitted.v1` and `scoring-engine` reacting to it. This kind of cross-module side-effect is exactly what per-module integration projects cannot test.
- Verifying within-module repository behaviour against a real database is still useful, but it no longer needs its own project per module — the same scenarios can be expressed inside a shared project that already has every module's schema in place.

The current state is also unusually cheap to change: across the ten modules in `backend/FootballCatch.sln` there are twenty scaffolded test projects (`{Module}.IntegrationTests` and `{Module}.Tests.Infrastructure`) and zero test files in any of them. Consolidating now costs nothing in terms of migrated tests; consolidating later would mean rewriting whatever has accumulated.

Alternatives considered:

| Option | Reason rejected |
|---|---|
| **Keep per-module `*.IntegrationTests` and `*.Tests.Infrastructure` (status quo)** | Cannot verify cross-module side-effects through the in-process dispatcher, which is the actual integration boundary after ADR-0006. Duplicates the Testcontainers + migration plumbing twenty times. Modular monolith already shares the database and the dispatcher, so splitting integration tests across modules adds friction without insulating anything. |
| **One integration test project per module *plus* a separate cross-module project** | Two parallel ways to write the "same" kind of test. Forces an arbitrary judgement on every new test about which project it belongs to. The within-module case is already covered by `{Module}.UnitTests` against application-layer ports, so the second project would be near-empty for most modules. |
| **Single shared integration tests project under `backend/common/tests` (selected)** | One place to write any test that needs a real database, regardless of how many modules it touches. Shared fixture, shared lifecycle, shared connection string. Mirrors the modular-monolith reality: one process, one database, one in-process dispatcher. |

## Decision

All backend integration tests live in a **single shared project** at:

```
backend/common/tests/FootballCatch.Common.IntegrationTests
```

- The project references every module's `Application`, `Infrastructure`, and `Migrations` projects, plus `FootballCatch.Common`. This is the minimum surface needed to exercise command/query handlers, repositories, and integration-event handlers across modules.
- A single Testcontainers PostgreSQL container is started per test run. Before any test executes, **every module's migrations are applied** against that container, producing the same shared schema the application runs against in production (see ADR-0007).
- Each module retains its own `DbContext` and its own distinct migrations history table (e.g. `__auth_migrations_history`, `__catalog_migrations_history`), as defined in ADR-0007. The shared fixture is responsible for invoking each module's migration pipeline in turn; it does not centralise migration ownership.
- A shared `DatabaseFixture` lives **inside this same project**. There is no separate `Tests.Infrastructure` project anywhere in the solution. Builders, object mothers, fakes, and base test classes live alongside the fixture in the shared project.
- Tests are organised by **cross-module scenario**, not by module — for example `Scenarios/PredictionSubmission/`, `Scenarios/UserRegistration/`, `Scenarios/MatchFinalization/`. A scenario folder contains the tests that exercise the end-to-end flow, regardless of how many modules participate.
- The twenty per-module `*.IntegrationTests` and `*.Tests.Infrastructure` projects are removed from the solution.

Unit testing is unaffected. Each module continues to ship its own `{Module}.UnitTests` for pure domain and application-layer logic; intra-module behaviour that does not require a real database stays there.

The `DatabaseFixture` implementation, container lifecycle wiring, and migration invocation are a follow-up task. No integration tests can be written until that fixture exists.

## Consequences

**Positive:**
- A single place to verify cross-module side-effects routed through the in-process dispatcher — the actual integration boundary in this architecture.
- One Testcontainers container, one connection string, one schema set-up path. The plumbing is written once and reused across every scenario.
- Solution layout is materially simpler: twenty empty test projects disappear; `FootballCatch.sln` shrinks accordingly.
- Aligns the test layout with the runtime layout. The system runs as one process against one database; its integration tests now do the same.
- Cross-module scenarios are first-class. They live in named scenario folders rather than being awkwardly split across two modules' test projects.

**Negative / obligations:**
- Every integration test class pays the cost of running every module's migrations against the container. With the project at its current size this is acceptable; if the migration set grows large enough to slow the suite materially, the fixture lifecycle (e.g. container reuse across the run) will need to be revisited.
- A single shared test project is a natural coupling magnet: it sees every module's `Application` and `Infrastructure`. The guideline to keep the project healthy is strict — **intra-module behaviour belongs in `{Module}.UnitTests`; only genuine cross-module scenarios (or scenarios that require a real database) go here.** Reviewers must enforce this.
- A `DatabaseFixture` implementation is a hard prerequisite. Until it exists, no integration test can be authored. This must be picked up as a follow-up before any cross-module scenario can be verified.
- Boundaries between modules are no longer reflected in the test project structure. Discipline shifts from project layout to folder convention (`Scenarios/...`) and code review.

## References
- ADR-0006 — Modular Monolith with In-Process Event Dispatcher.
- ADR-0007 — Shared Database, Shared Schema.
- `docs/process/testing.md` — testing strategy, including the shared integration tests project.
