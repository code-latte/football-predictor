# Database Migrations

All backend microservices use **FluentMigrator** to manage PostgreSQL schema migrations.  
Migrations live inside each service under `src/Infrastructure/Migrations/` and run automatically on startup.

---

## Required packages

Add to each microservice's `.csproj`:

```xml
<PackageReference Include="FluentMigrator" Version="5.*" />
<PackageReference Include="FluentMigrator.Runner" Version="5.*" />
<PackageReference Include="FluentMigrator.Runner.Postgres" Version="5.*" />
```

---

## Setup in Program.cs

```csharp
// Register FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(
            builder.Configuration.GetConnectionString("Default"))
        .ScanIn(typeof(Program).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

// Run pending migrations on startup
using var scope = app.Services.CreateScope();
scope.ServiceProvider
    .GetRequiredService<IMigrationRunner>()
    .MigrateUp();
```

---

## Naming convention

| Part | Rule | Example |
|---|---|---|
| Version | Timestamp `YYYYMMDDHHmm` (12 digits) | `202401150930` |
| Class | `M{version}_{PascalCaseDescription}` | `M202401150930_CreatePredictionsTable` |
| File | Same as class name | `M202401150930_CreatePredictionsTable.cs` |
| Folder | `src/Infrastructure/Migrations/` | — |

Using a timestamp version guarantees global ordering without coordination across the team.

---

## Examples

### 1. Create a table (initial schema)

```csharp
[Migration(202401150930, "Create predictions table")]
public class M202401150930_CreatePredictionsTable : Migration
{
    public override void Up()
    {
        Create.Table("predictions")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("match_id").AsGuid().NotNullable()
            .WithColumn("predicted_home").AsInt32().NotNullable()
            .WithColumn("predicted_away").AsInt32().NotNullable()
            .WithColumn("submitted_at").AsDateTimeOffset().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("predictions");
    }
}
```

### 2. Add a column

```csharp
[Migration(202402010800, "Add locked_at column to predictions")]
public class M202402010800_AddLockedAtToPredictions : Migration
{
    public override void Up()
    {
        Alter.Table("predictions")
            .AddColumn("locked_at").AsDateTimeOffset().Nullable();
    }

    public override void Down()
    {
        Delete.Column("locked_at").FromTable("predictions");
    }
}
```

### 3. Add an index

```csharp
[Migration(202402150900, "Add index on predictions user_id")]
public class M202402150900_AddIndexPredictionsUserId : Migration
{
    public override void Up()
    {
        Create.Index("ix_predictions_user_id")
            .OnTable("predictions")
            .OnColumn("user_id").Ascending();
    }

    public override void Down()
    {
        Delete.Index("ix_predictions_user_id").OnTable("predictions");
    }
}
```

### 4. Raw SQL (when the fluent API is insufficient)

```csharp
[Migration(202403010700, "Add check constraint on predicted scores")]
public class M202403010700_AddScoreCheckConstraint : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            ALTER TABLE predictions
            ADD CONSTRAINT chk_scores_non_negative
            CHECK (predicted_home >= 0 AND predicted_away >= 0);
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            ALTER TABLE predictions
            DROP CONSTRAINT chk_scores_non_negative;
        ");
    }
}
```

---

## Best practices

- **Never modify** a migration that has been applied to any environment — add a new one instead.  
- **Always implement `Down()`** — it enables rollback in production incidents.  
- **One logical change per migration** — keep them small and focused.  
- **Use `Execute.Sql()` only** when the fluent API cannot express the change.  
- **Test migrations** in integration tests using Testcontainers (PostgreSQL container).  
- **Do not put business logic** in migrations — schema changes only.
