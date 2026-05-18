# ADR 0005: Use EF Core Migrations for Database Schema Management

## Status

Accepted

## Context

Each module owns its PostgreSQL database and requires a consistent, version-controlled mechanism to evolve the schema. ADR-0003 chose FluentMigrator for this role.

Since then the team has adopted Entity Framework Core (Npgsql provider) as the primary data access layer across all modules. Running two separate schema-management tools — EF Core for querying and FluentMigrator for migrations — introduced friction: developers had to keep two mental models in sync, and there was no straightforward way to share a single `DbContext` between the running application and the integration test suite.

Alternatives reconsidered:

- **FluentMigrator (status quo):** Retained as a dedicated migration tool. Rejected because it duplicates the schema definition already present in EF Core entity configurations, and it cannot be used directly by Testcontainers-based integration tests to build the database from scratch.
- **Flyway / DbUp:** SQL-file-based tools. Rejected because they require maintaining raw SQL alongside C# entity mappings, increasing drift risk.
- **EF Core Migrations:** Code-first migrations generated from the `DbContext`. Selected — see Decision below.

## Decision

All modules use **EF Core Migrations** (via `Npgsql.EntityFrameworkCore.PostgreSQL`) to manage their PostgreSQL schema.

Each module contains a dedicated `{Module}.Migrations` project under `src/`. This project:

- Holds the EF Core `DbContext` configured for Npgsql, plus all generated `Migration` classes.
- Has no dependency on any domain or application layer — only EF Core and Npgsql packages.
- Is referenced by `{Service}.Infrastructure` (for DI registration and startup migration execution) and by `{Service}.Tests.Infrastructure` (so integration tests can apply migrations to a real containerised database).

Migrations are generated with the standard EF Core tooling:

```bash
dotnet ef migrations add {YYYYMMDDHHmm}_{PascalCaseDescription} \
  --project src/{Service}.Migrations \
  --startup-project src/{Service}.Api
```

On startup, the application applies pending migrations by calling `dbContext.Database.MigrateAsync()` (or an equivalent hosted service wrapper).

## Consequences

**Positive:**

- A single `DbContext` definition serves both query/command execution and schema management — no duplication.
- The `{Service}.Migrations` project can be referenced directly from `{Service}.Tests.Infrastructure`, allowing integration tests to build the database schema from scratch without any mocking or manual SQL scripts.
- `dotnet ef migrations script` generates reviewable SQL that can be inspected in pull requests and applied manually in production if needed.
- EF Core tooling (`dotnet ef`) is already familiar to the team and is part of the standard .NET ecosystem.

**Negative / obligations:**

- Applied migrations must never be deleted or modified — only new migrations may be added.
- The `{Service}.Migrations` project must remain free of domain and application dependencies to avoid circular references.
- The `dotnet ef` CLI tool (`dotnet-ef`) must be installed in CI environments that generate or validate migrations.
- Developers must review generated migration files before committing — EF Core occasionally generates unnecessary or incorrect SQL for complex mappings.
