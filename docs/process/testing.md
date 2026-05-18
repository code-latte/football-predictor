# Testing Strategy

All backend modules follow a three-project test structure. Every test project lives under `tests/` at the module root.

---

## Test project structure

```
tests/
  {Module}.UnitTests/              ← Pure domain and application logic, no I/O
  {Module}.IntegrationTests/       ← Tests that hit a real PostgreSQL database
  {Module}.Tests.Infrastructure/   ← Shared helpers: DB setup, fakes, builders
```

---

## Project purposes and dependencies

### `{Module}.UnitTests`

Tests domain entities, value objects, domain services, and application use cases in isolation. No database, no network, no file system.

**References:**
- `{Module}.Domain`
- `{Module}.Application`
- `{Module}.Tests.Infrastructure` (optional — for shared object-mother / builder helpers)

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

### `{Module}.IntegrationTests`

Tests that verify module behaviour against a real PostgreSQL database spun up by Testcontainers. These tests cover repository implementations, EF Core queries, and end-to-end command/query handlers that touch the database.

**References:**
- `{Module}.Application`
- `{Module}.Infrastructure`
- `{Module}.Tests.Infrastructure`

**NuGet packages:**
```xml
<PackageReference Include="NUnit" Version="4.*" />
<PackageReference Include="NUnit3TestAdapter" Version="4.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" />
```

**Conventions:**
- Never mock the database — use a real PostgreSQL container via Testcontainers.
- Obtain the container connection string from `{Module}.Tests.Infrastructure.DatabaseFixture`.
- Each test class that needs the database implements `IClassFixture<DatabaseFixture>` (or inherits a base class that does).
- At least one integration test must exist per module.

> The move to a shared database (see `docs/adr/0007-shared-database-shared-schema.md`) is transparent to test setup. Each `DatabaseFixture` still spins up a fresh PostgreSQL container per fixture and calls `MigrateAsync()` on the module's `DbContext`.

---

### `{Module}.Tests.Infrastructure`

Shared test infrastructure consumed by both `{Module}.UnitTests` and `{Module}.IntegrationTests`. Contains no test classes — only infrastructure.

**References:**
- `{Module}.Migrations` — to call `dbContext.Database.MigrateAsync()` against the containerised database.
- `{Module}.Domain` (for builder / object-mother types).

**NuGet packages:**
```xml
<PackageReference Include="NUnit" Version="4.*" />
<PackageReference Include="NSubstitute" Version="5.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
```

**What goes here:**

| Type | Description |
|---|---|
| `DatabaseFixture` | Manages the Testcontainers PostgreSQL container lifecycle (start/stop). Exposes the connection string and calls `dbContext.Database.MigrateAsync()` before any test runs. |
| Fake implementations | In-memory fakes for ports that integration tests should not call for real (e.g. `FakeEventPublisher`, `FakeEmailSender`, `FakeClock`). |
| Object mothers / builders | Fluent builder classes and static factory methods for constructing valid domain objects and EF entities in tests (e.g. `PredictionBuilder`, `MatchMother`). |
| Base test fixtures | Optional abstract base classes that wire up `DatabaseFixture` and common setup/teardown. |

---

## Dependency graph summary

```
{Module}.UnitTests
  → {Module}.Domain
  → {Module}.Application
  → {Module}.Tests.Infrastructure (optional)

{Module}.IntegrationTests
  → {Module}.Application
  → {Module}.Infrastructure
  → {Module}.Tests.Infrastructure

{Module}.Tests.Infrastructure
  → {Module}.Migrations
  → {Module}.Domain
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
