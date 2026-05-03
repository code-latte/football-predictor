# Database Migrations

All backend microservices use **FluentMigrator** to manage PostgreSQL schema changes.  
EF Core (Npgsql provider) is the ORM for querying and persistence — it is **not** used for schema management.  
The decision to reinstate FluentMigrator is recorded in `docs/adr/0006-reinstate-fluentmigrator.md`.

---

## Project structure

Each microservice exposes a dedicated migrations project under `src/`:

```
src/
  {Service}.Domain/
  {Service}.Application/
  {Service}.Infrastructure/     ← DbContext lives here
  {Service}.Api/
  {Service}.Migrations/         ← FluentMigrator Migration classes only
```

**`{Service}.Migrations` rules:**
- Contains only FluentMigrator `Migration` subclasses. No `DbContext`, no model snapshot, no Designer files.
- Has **no** project reference to `{Service}.Domain`, `{Service}.Application`, or `{Service}.Infrastructure`.
- Depends only on `FluentMigrator`, `FluentMigrator.Runner`, and `FluentMigrator.Runner.Postgres` NuGet packages.
- Is referenced by `{Service}.Api` (to register the runner) and by `{Service}.Tests.Infrastructure` (to apply migrations in the Testcontainers integration test setup).

---

## Required NuGet packages

Add to `{Service}.Migrations.csproj`:

```xml
<PackageReference Include="FluentMigrator" Version="6.*" />
<PackageReference Include="FluentMigrator.Runner" Version="6.*" />
<PackageReference Include="FluentMigrator.Runner.Postgres" Version="6.*" />
<PackageReference Include="Npgsql" Version="9.*" />
```

No `dotnet ef` CLI tool is required — FluentMigrator migration classes are plain C# with no code-generation step.

---

## Migration class structure

Each migration is a class that:
- Is decorated with `[Migration(YYYYMMDDHHmm)]` — the timestamp is the version number.
- Extends `FluentMigrator.Migration`.
- Implements `Up()` (apply) and `Down()` (rollback) using the FluentMigrator fluent API.

```csharp
using FluentMigrator;

namespace FootballCatch.Catalog.Migrations;

[Migration(202401150930)]
public sealed class CreateCompetitionsTable : Migration
{
    public override void Up()
    {
        Create.Table("competitions")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("country").AsString(100).NotNullable()
            .WithColumn("season").AsString(20).NotNullable()
            .WithColumn("logo_url").AsString(500).Nullable()
            .WithColumn("external_id").AsString(100).Nullable().Unique()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at_utc").AsDateTime().NotNullable()
            .WithColumn("updated_at_utc").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("competitions");
    }
}
```

---

## Naming convention

| Part | Rule | Example |
|---|---|---|
| Timestamp | `YYYYMMDDHHmm` (12 digits) | `202401150930` |
| `[Migration(...)]` attribute | Same timestamp as a `long` literal | `[Migration(202401150930)]` |
| Class name | `{PascalCaseDescription}` | `CreateCompetitionsTable` |
| File name | `{timestamp}_{PascalCaseDescription}.cs` | `202401150930_CreateCompetitionsTable.cs` |
| Folder | `{Service}.Migrations/` (root of the project) | — |
| Table names | PascalCase | `Competitions`, `Teams` |
| Column names | PascalCase | `Id`, `Name`, `CountryIsoCode`, `CreatedAtUtc` |
| Index names | `IX_{Table}_{Column(s)}` | `IX_Competitions_ExternalId` |

PascalCase for table and column names matches C# property names directly. No column-name mapping is needed in FluentMigrator migration classes — the name in the migration is the name in the schema.

Using a timestamp version guarantees global ordering without coordination across the team.

---

## Database creation before migrations

FluentMigrator operates only on tables and schema within an existing database — it does not create the PostgreSQL database itself. A `DatabaseInitializer` startup utility must run before the FluentMigrator runner to ensure the target database exists.

```csharp
// {Service}.Api/DatabaseInitializer.cs
using Npgsql;

internal static class DatabaseInitializer
{
    internal static async Task EnsureCreatedAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var dbName = builder.Database!;

        // Connect to the maintenance database to issue CREATE DATABASE
        builder.Database = "postgres";

        await using var conn = new NpgsqlConnection(builder.ToString());
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT 1 FROM pg_database WHERE datname = '{dbName}'
            """;

        var exists = await cmd.ExecuteScalarAsync() is not null;
        if (!exists)
        {
            cmd.CommandText = $"""CREATE DATABASE "{dbName}" """;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
```

Call `DatabaseInitializer.EnsureCreatedAsync` in `Program.cs` before wiring the FluentMigrator runner.

---

## Wiring the runner in DI

Register the FluentMigrator runner in `{Service}.Api/Program.cs` (or a shared Infrastructure extension method) and execute it at startup:

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

`MigrateUp()` is idempotent — it records applied versions in FluentMigrator's `VersionInfo` table and skips any already-applied migration.

---

## Best practices

- **Never delete or modify** a migration that has been applied to any environment — add a new migration instead.
- **One logical change per migration** — keep them small and focused.
- **Schema changes only** — no business logic, no seed data, no application-layer calls inside a migration.
- **Keep EF configurations in sync** — the EF entity configurations in `{Service}.Infrastructure/Persistence/Configurations/` define the same schema as the FluentMigrator migrations. There is no automated check; review both when adding or altering a column.
- **Test migrations in integration tests** — `{Service}.Tests.Infrastructure` references `{Service}.Migrations`, calls `DatabaseInitializer.EnsureCreatedAsync`, and then runs the FluentMigrator runner against a Testcontainers PostgreSQL container to verify the schema builds from scratch on every test run.
