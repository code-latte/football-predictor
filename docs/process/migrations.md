# Database Migrations

All backend modules use **EF Core Migrations** (Npgsql provider) to manage PostgreSQL schema changes.
Each module contains a dedicated `{Module}.Migrations` project that holds the `DbContext` and all generated migration classes. Migrations are applied automatically on startup.

The decision to adopt EF Core Migrations and supersede FluentMigrator is recorded in `docs/adr/0005-use-efcore-migrations.md`.

The platform uses a single PostgreSQL database with a shared schema; module isolation is enforced in code (see `docs/adr/0007-shared-database-shared-schema.md`).

---

## Project structure

Each module exposes a dedicated migrations project under `src/`:

```
src/
  {Module}.Domain/
  {Module}.Application/
  {Module}.Infrastructure/
  {Module}.Api/
  {Module}.Migrations/        ← EF Core DbContext + all Migration classes
```

**`{Module}.Migrations` rules:**

- Contains the EF Core `DbContext` configured for Npgsql and all generated `Migration` classes — nothing else.
- Has no dependency on `{Module}.Domain` or `{Module}.Application`. Only EF Core and Npgsql packages.
- Referenced by `{Module}.Infrastructure` (DI registration, startup execution) and by `{Module}.Tests.Infrastructure` (integration test database setup).

---

## Shared database, table-prefix convention

All modules' `*.Migrations` projects target the **same PostgreSQL database** (one database, one schema). To prevent table-name collisions across modules, **every table is prefixed with the module short name**:

| Module           | Prefix           | Example tables                           |
| ---------------- | ---------------- | ---------------------------------------- |
| `auth`           | `auth_`          | `auth_users`, `auth_refresh_tokens`      |
| `catalog`        | `catalog_`       | `catalog_competitions`, `catalog_teams`  |
| `fixtures`       | `fixtures_`      | `fixtures_matches`                       |
| `predictions`    | `predictions_`   | `predictions_predictions`                |
| `leagues`        | `leagues_`       | `leagues_leagues`, `leagues_memberships` |
| `scoring-engine` | `scoring_`       | `scoring_user_scores`                    |
| `stats`          | `stats_`         | `stats_leaderboards`                     |
| `notifications`  | `notifications_` | `notifications_outbox`                   |
| `user-profile`   | `profile_`       | `profile_profiles`                       |

Each module's `DbContext` retains its own migration history table to keep migration tracking from colliding. This is configured via EF Core's `migrationsHistoryTable` option (e.g. `__catalog_migrations_history`, `__auth_migrations_history`).

> Configuring `migrationsHistoryTable` per module is a follow-up task — not implemented in this PR.

---

## Required NuGet packages

Add to `{Module}.Migrations.csproj`:

```xml
<PackageReference Include="FluentMigrator" Version="6.*" />
<PackageReference Include="FluentMigrator.Runner" Version="6.*" />
<PackageReference Include="FluentMigrator.Runner.Postgres" Version="6.*" />
<PackageReference Include="Npgsql" Version="9.*" />
```

No `dotnet ef` CLI tool is required — FluentMigrator migration classes are plain C# with no code-generation step.

---

## Adding a migration

Run from the repository root (or the module folder):

```bash
dotnet ef migrations add {YYYYMMDDHHmm}_{PascalCaseDescription} \
  --project src/{Module}.Migrations \
  --startup-project src/{Module}.Api
```

Example:

```bash
dotnet ef migrations add 202401150930_CreatePredictionsTable \
  --project src/Predictions.Migrations \
  --startup-project src/Predictions.Api
```

EF Core generates three files inside `{Module}.Migrations/Migrations/`:

- `{timestamp}_{Description}.cs` — the `Up` and `Down` migration methods.
- `{timestamp}_{Description}.Designer.cs` — snapshot metadata used by EF tooling.
- `{ModuleName}DbContextModelSnapshot.cs` — the cumulative model snapshot (updated automatically).

Always review the generated `.cs` file before committing to confirm it reflects the intended schema change.

---

## Naming convention

| Part                    | Rule                                   | Example                               |
| ----------------------- | -------------------------------------- | ------------------------------------- |
| Timestamp               | `YYYYMMDDHHmm` (12 digits)             | `202401150930`                        |
| Migration name argument | `{timestamp}_{PascalCaseDescription}`  | `202401150930_CreatePredictionsTable` |
| Generated class name    | Same as the name argument              | `202401150930_CreatePredictionsTable` |
| Folder                  | `{Module}.Migrations/Migrations/`      | —                                     |
| Table name              | `{moduleShortName}_{snake_case_table}` | `catalog_teams`                       |

Using a timestamp prefix guarantees global ordering without coordination across the team.

---

## Applying migrations on startup

In `{Module}.Api`, register and execute migrations via a hosted service or directly in `Program.cs`:

```csharp
// Register the runner
builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(CreateCompetitionsTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

// Ensure database exists, then apply pending migrations
await DatabaseInitializer.EnsureCreatedAsync(connectionString);

using var scope = app.Services.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();
```

`MigrateAsync()` is idempotent — it applies only migrations that have not yet been recorded in the module's migration history table.

---

## Generating a SQL script (for review or manual production apply)

```bash
dotnet ef migrations script \
  --project src/{Module}.Migrations \
  --startup-project src/{Module}.Api \
  --output migrations.sql \
  --idempotent
```

The `--idempotent` flag wraps each statement in an existence check, making the script safe to run against databases at any migration level.

---

## Best practices

- **Never delete or modify** a migration that has been applied to any environment — add a new migration instead.
- **One logical change per migration** — keep them small and focused.
- **Schema changes only** — no business logic, no seed data, no application-layer calls inside a migration.
- **Always prefix table names with the module short name** — this is the only mechanism preventing cross-module collisions in the shared schema.
- **Test migrations in integration tests** — `{Module}.Tests.Infrastructure` references `{Module}.Migrations` and calls `MigrateAsync()` against a Testcontainers PostgreSQL container to verify the schema builds from scratch on every test run.
- **Commit the model snapshot** — `{ModuleName}DbContextModelSnapshot.cs` must always be committed alongside its migration.
