# ADR 0007: Shared Database, Shared Schema

## Status
Accepted

## Context

ADR-0002 mandated one PostgreSQL database per microservice, with cross-service data replicated via integration events and projections. That decision was tightly coupled to the original microservices architecture (ADR-0001), which has since been superseded by the modular monolith pivot (ADR-0006).

With a single backend deployable, the database-per-service rule no longer serves its original goals: there is no longer a cross-process boundary to enforce, no independently deployed service that needs its own datastore, and no real benefit to running nine PostgreSQL instances on the same VPS. The cost of the original rule — nine connection strings, nine migration histories, nine sets of credentials, nine sets of backups, plus the data-replication projections needed to read another module's data — is paid every day in development friction and operational complexity, for almost no benefit.

Alternatives considered:

| Option | Reason rejected |
|---|---|
| **One database per module (status quo, ADR-0002)** | Drops nine PostgreSQL instances onto a single VPS for no operational gain after the monolith pivot. Forces event-driven projections for cross-module reads that are trivial in a shared database. |
| **One database, one PostgreSQL schema per module** | Adds the operational complexity of multiple schemas (search paths, per-schema permissions, cross-schema EF Core configuration) without solving any concrete problem. EF Core support for per-schema migration histories is workable but adds friction. |
| **One database, shared schema, table-prefix convention per module (selected)** | Lowest operational overhead. Module boundaries are enforced in code, where they always belonged in practice. Table prefixes make ownership obvious to anyone reading the schema. |

## Decision

The backend uses **one PostgreSQL database with a shared schema**.

- All modules connect to the same database.
- Module boundaries are enforced **in code at the module boundary**: a module owns its tables, and modules do not directly query another module's tables. Cross-module reads go through the owning module's application layer or — for asynchronous flows — through the in-process event dispatcher (see ADR-0006).
- To avoid table-name collisions across modules, **every table is prefixed with the module short name**. Examples:
  - `auth_users`, `auth_refresh_tokens`, `auth_device_tokens`
  - `catalog_competitions`, `catalog_teams`
  - `fixtures_matches`, `fixtures_match_events`
  - `predictions_predictions`
  - `leagues_leagues`, `leagues_memberships`
- Each module's `*.Migrations` project continues to own its own `DbContext` and its own migration history. To keep migration tracking from colliding, each module's `DbContext` configures a distinct migration history table (e.g. `__auth_migrations_history`, `__catalog_migrations_history`) via EF Core's `migrationsHistoryTable` option. Configuring `migrationsHistoryTable` per module is a follow-up task — not implemented in this PR.

## Consequences

**Positive:**
- Operational simplicity: one PostgreSQL instance, one connection string, one backup target.
- Local development is faster and lighter: starting the platform requires one database, not nine.
- Cross-module reads in administrative or reporting contexts become trivial; the previous need for projection tables to read another module's data disappears.
- Schema ownership is visible at a glance: anyone reading the schema can tell which module owns a table from its prefix.

**Negative / obligations:**
- Schema-level enforcement of module boundaries is gone. Nothing in the database prevents one module from querying another module's tables directly. This discipline now lives entirely in **code review** and the **in-process dispatcher contract** described in ADR-0006.
- A breach of the boundary (e.g. `LeaguesRepository` reading from `auth_users` directly) is silent — there is no compile-time or runtime guard. Reviewers must catch it.
- All modules share the same database resource. Heavy workloads in one module can affect the others. This trade-off is acceptable at the project's current scale.
- A future split back into separate databases would require migrating each module's tables (already prefixed, so identifying them is straightforward) into its own database and reintroducing event-driven projections for cross-module reads.

## References
- ADR-0002 — Database per Microservice (superseded by this ADR).
- ADR-0006 — Modular Monolith with In-Process Event Dispatcher.
- ADR-0005 — Use EF Core Migrations for Database Schema Management.
- `docs/process/migrations.md` — EF Core migration conventions, including the per-module migration history table.
