# Database Migrations

All backend microservices use **EF Core Migrations** (Npgsql provider) to manage PostgreSQL schema changes.  
Each service contains a dedicated `{Service}.Migrations` project that holds the `DbContext` and all generated migration classes. Migrations are applied automatically on startup.

The decision to adopt EF Core Migrations and supersede FluentMigrator is recorded in `docs/adr/0005-use-efcore-migrations.md`.

---

## Project structure

Each microservice exposes a dedicated migrations project under `src/`:

```
src/
  {Service}.Domain/
  {Service}.Application/
  {Service}.Infrastructure/
  {Service}.Api/
  {Service}.Migrations/        ← EF Core DbContext + all Migration classes
```

**`{Service}.Migrations` rules:**
- Contains the EF Core `DbContext` configured for Npgsql and all generated `Migration` classes — nothing else.
- Has no dependency on `{Service}.Domain` or `{Service}.Application`. Only EF Core and Npgsql packages.
- Referenced by `{Service}.Infrastructure` (DI registration, startup execution) and by `{Service}.Tests.Infrastructure` (integration test database setup).

---

## Required NuGet packages

Add to `{Service}.Migrations.csproj`:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

The `dotnet ef` CLI tool must also be available. Install it once per machine:

```bash
dotnet tool install --global dotnet-ef
```

---

## Adding a migration

Run from the repository root (or the service folder):

```bash
dotnet ef migrations add {YYYYMMDDHHmm}_{PascalCaseDescription} \
  --project src/{Service}.Migrations \
  --startup-project src/{Service}.Api
```

Example:

```bash
dotnet ef migrations add 202401150930_CreatePredictionsTable \
  --project src/Predictions.Migrations \
  --startup-project src/Predictions.Api
```

EF Core generates three files inside `{Service}.Migrations/Migrations/`:
- `{timestamp}_{Description}.cs` — the `Up` and `Down` migration methods.
- `{timestamp}_{Description}.Designer.cs` — snapshot metadata used by EF tooling.
- `{ServiceName}DbContextModelSnapshot.cs` — the cumulative model snapshot (updated automatically).

Always review the generated `.cs` file before committing to confirm it reflects the intended schema change.

---

## Naming convention

| Part | Rule | Example |
|---|---|---|
| Timestamp | `YYYYMMDDHHmm` (12 digits) | `202401150930` |
| Migration name argument | `{timestamp}_{PascalCaseDescription}` | `202401150930_CreatePredictionsTable` |
| Generated class name | Same as the name argument | `202401150930_CreatePredictionsTable` |
| Folder | `{Service}.Migrations/Migrations/` | — |

Using a timestamp prefix guarantees global ordering without coordination across the team.

---

## Applying migrations on startup

In `{Service}.Api`, register and execute migrations via a hosted service or directly in `Program.cs`:

```csharp
// In Program.cs — register the DbContext from the Migrations project
builder.Services.AddDbContext<PredictionsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Apply pending migrations on startup
var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PredictionsDbContext>();
await dbContext.Database.MigrateAsync();
```

`MigrateAsync()` is idempotent — it applies only migrations that have not yet been recorded in the `__EFMigrationsHistory` table.

---

## Generating a SQL script (for review or manual production apply)

```bash
dotnet ef migrations script \
  --project src/{Service}.Migrations \
  --startup-project src/{Service}.Api \
  --output migrations.sql \
  --idempotent
```

The `--idempotent` flag wraps each statement in an existence check, making the script safe to run against databases at any migration level.

---

## Best practices

- **Never delete or modify** a migration that has been applied to any environment — add a new migration instead.
- **Review generated SQL** before committing. EF Core can generate unexpected statements for complex mappings (e.g. renamed columns, table splits).
- **One logical change per migration** — keep them small and focused.
- **Schema changes only** — no business logic, no seed data, no application-layer calls inside a migration.
- **Test migrations in integration tests** — `{Service}.Tests.Infrastructure` references `{Service}.Migrations` and calls `MigrateAsync()` against a Testcontainers PostgreSQL container to verify the schema builds from scratch on every test run.
- **Commit the model snapshot** — `{ServiceName}DbContextModelSnapshot.cs` must always be committed alongside its migration.
