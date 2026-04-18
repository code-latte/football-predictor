# Testing Strategy

All backend microservices follow a three-project test structure. Every test project lives under `tests/` at the service root.

---

## Test project structure

```
tests/
  {Service}.UnitTests/              ← Pure domain and application logic, no I/O
  {Service}.IntegrationTests/       ← Tests that hit a real PostgreSQL database
  {Service}.Tests.Infrastructure/   ← Shared helpers: DB setup, fakes, builders
```

---

## Project purposes and dependencies

### `{Service}.UnitTests`

Tests domain entities, value objects, domain services, and application use cases in isolation. No database, no network, no file system.

**References:**
- `{Service}.Domain`
- `{Service}.Application`
- `{Service}.Tests.Infrastructure` (optional — for shared object-mother / builder helpers)

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

### `{Service}.IntegrationTests`

Tests that verify service behaviour against a real PostgreSQL database spun up by Testcontainers. These tests cover repository implementations, EF Core queries, and end-to-end command/query handlers that touch the database.

**References:**
- `{Service}.Application`
- `{Service}.Infrastructure`
- `{Service}.Tests.Infrastructure`

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
- Obtain the container connection string from `{Service}.Tests.Infrastructure.DatabaseFixture`.
- Each test class that needs the database implements `IClassFixture<DatabaseFixture>` (or inherits a base class that does).
- At least one integration test must exist per service.

---

### `{Service}.Tests.Infrastructure`

Shared test infrastructure consumed by both `{Service}.UnitTests` and `{Service}.IntegrationTests`. Contains no test classes — only infrastructure.

**References:**
- `{Service}.Migrations` — to call `dbContext.Database.MigrateAsync()` against the containerised database.
- `{Service}.Domain` (for builder / object-mother types).

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
{Service}.UnitTests
  → {Service}.Domain
  → {Service}.Application
  → {Service}.Tests.Infrastructure (optional)

{Service}.IntegrationTests
  → {Service}.Application
  → {Service}.Infrastructure
  → {Service}.Tests.Infrastructure

{Service}.Tests.Infrastructure
  → {Service}.Migrations
  → {Service}.Domain
```

---

## Frontend tests

| Level | Tool | Requirement |
|---|---|---|
| Unit | Jest + React Testing Library | Component and hook logic in isolation |
| E2E | Playwright or Cypress (web/backoffice), Detox (mobile) | Critical paths: submit prediction → scoring → leaderboard update |

---

## Contract tests

Integration events are validated against the contracts defined in `backend/common`. A CI check confirms that event shapes published and consumed by each service match the common contracts package. This check must pass before any PR is merged.

---

## Continuous integration

All tests run in GitHub Actions on every pull request. **A PR cannot be merged if any test fails.**
