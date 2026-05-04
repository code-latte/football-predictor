# ADR 0006: Reinstate FluentMigrator as the Schema Migration Tool

## Status
Accepted

## Context
ADR-0003 established FluentMigrator as the migration tool for all microservices. ADR-0005 superseded it by adopting EF Core Migrations, motivated by the desire to unify schema management with the EF Core ORM already in use for querying and persistence.

The EF Core Migrations experiment surfaced two practical problems:

1. **Tooling coupling.** Generating migrations requires `dotnet ef migrations add`, which needs the `dotnet-ef` CLI, a startup project, and a discoverable `DbContext` at design time. This added CI tooling requirements and made the migration generation path sensitive to project reference changes.
2. **Project structure tension.** The `{Service}.Migrations` project had to reference `{Service}.Infrastructure` (to discover `IEntityTypeConfiguration<T>` classes via `ApplyConfigurationsFromAssembly`), while `{Service}.Infrastructure` had to reference `{Service}.Migrations` (to register the `DbContext` for DI). This produced a circular dependency that was resolved only by inverting the reference direction — leaving the `DbContext` in Migrations and having Infrastructure depend on it, which is architecturally backwards.

Alternatives reconsidered:

- **EF Core Migrations (status quo at the time of ADR-0005):** Reverted for the reasons above.
- **Flyway / DbUp:** SQL-file-based tools. Still rejected — they require maintaining raw SQL alongside C# entity mappings.
- **FluentMigrator:** Reinstated. Migration classes are plain C# with no tooling-generated artifacts. The `{Service}.Migrations` project depends only on FluentMigrator and Npgsql packages — no reference to Infrastructure or Domain. The `DbContext` remains in `{Service}.Infrastructure` where it belongs.

## Decision
**FluentMigrator** handles all PostgreSQL schema migrations across all microservices. EF Core (Npgsql provider) is retained as the ORM for querying and persistence — entities, configurations, repositories, and `DbContext` all live in `{Service}.Infrastructure`. EF Core is **not** used for schema management.

The `{Service}.Migrations` project:
- Contains only FluentMigrator `Migration` subclasses.
- References `FluentMigrator`, `FluentMigrator.Runner`, and `FluentMigrator.Runner.Postgres` — nothing else from this solution.
- Has no project reference to `{Service}.Infrastructure`, `{Service}.Domain`, or `{Service}.Application`.

Because FluentMigrator does not create the PostgreSQL database itself (only tables and schema within an existing database), a `DatabaseInitializer` startup utility must open a connection to the `postgres` maintenance database and execute `CREATE DATABASE IF NOT EXISTS "<dbname>"` before the FluentMigrator runner executes. See `docs/process/migrations.md` for the implementation pattern.

Migrations are applied automatically on startup. No external tooling is required in CI/CD.

## Consequences

**Positive:**
- No tooling-generated artifacts — no `DbContextModelSnapshot`, no `.Designer.cs` files, no `dotnet ef` CLI dependency.
- Clean dependency graph: Migrations → nothing in this solution; Infrastructure → nothing in Migrations.
- The `DbContext` lives in Infrastructure, which is the correct home for a persistence-layer concern.
- Integration tests build the schema via the FluentMigrator runner, keeping the same startup code path as production.

**Negative / obligations:**
- Applied migrations must never be deleted or modified — add a new migration instead.
- The schema defined in FluentMigrator migrations must stay in sync with the EF entity configurations in Infrastructure. There is no automated check for this; developers are responsible for keeping them aligned.
- Each service must include the `DatabaseInitializer` startup step — FluentMigrator will fail if the target database does not exist.
