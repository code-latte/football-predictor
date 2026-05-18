# ADR 0003: Use FluentMigrator for Database Migrations

## Status
Accepted — reaffirmed by ADR-0006

## Context
Each module owns its PostgreSQL database. We need a consistent, version-controlled way to manage schema changes across all modules. Options considered: EF Core Migrations, Flyway, DbUp, FluentMigrator.

## Decision
We will use **FluentMigrator** because:
- Code-first, strongly typed migrations in C# — no separate SQL files to manage.  
- Fits naturally in .NET 10 / DI / hosted services.  
- Built-in `Up`/`Down` support for rollbacks.  
- Supports PostgreSQL out of the box.  
- Migrations run automatically on startup — no external tooling required in CI/CD.

## Consequences
- All modules must include the FluentMigrator NuGet packages.  
- Migrations are version-controlled alongside service code.  
- Applied migrations must never be modified — only new migrations added.  
- Each module maintains its own migration history via FluentMigrator's `VersionInfo` table.
