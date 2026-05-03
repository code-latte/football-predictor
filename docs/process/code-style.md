# Code Style Guidelines

We enforce consistent coding standards across all languages.

## C# (.NET)
- Use **PascalCase** for classes, methods, and properties.  
- Use **camelCase** for local variables and parameters.  
- Use `record struct` for strongly typed IDs.  
- Organize projects in layers: Domain, Application, Infrastructure, API.  
- Unit tests must follow `[UnitOfWork_StateUnderTest_ExpectedBehavior]` naming convention.

### Infrastructure layer — persistence conventions

These rules apply to all microservices. The catalog service (`backend/catalog`) is the reference implementation.

#### Database naming

- **Table names:** PascalCase — e.g. `Competitions`, `Teams`.
- **Column names:** PascalCase — e.g. `Id`, `Name`, `CountryIsoCode`, `SeasonStartYear`, `ExternalId`, `CreatedAtUtc`.
- **Index names:** `IX_{Table}_{Column(s)}` — e.g. `IX_Competitions_ExternalId`, `IX_Teams_ExternalId`.

PascalCase matches C# property names directly, so EF Core entity properties map to same-name columns with no `HasColumnName` conversion needed in `IEntityTypeConfiguration` classes.

---

#### EF entities defined in Infrastructure

The Infrastructure layer defines its own persistence classes — `{Aggregate}Entity` — that map directly to the database schema. Domain aggregates are **never** annotated with EF Core attributes and are **never** registered as EF entity types.

**Rationale:** Domain aggregates use value objects, private setters, and constructors that enforce invariants. EF Core needs a parameterless constructor and direct property access that would either force public setters onto the domain model or require complex shadow-property workarounds. Keeping EF entities as plain, dumb data containers in Infrastructure preserves the domain's design freedom.

**Folder:** `{Service}.Infrastructure/Persistence/Entities/`

```csharp
// FootballCatch.Catalog.Infrastructure/Persistence/Entities/CompetitionEntity.cs
namespace FootballCatch.Catalog.Infrastructure.Persistence.Entities;

public sealed class CompetitionEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? ExternalId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
```

Value objects (e.g. `CompetitionName`, `Country`, `Season`) are stored as their primitive representations. The mapper (see below) reconstructs them.

---

#### EF configurations defined in Infrastructure

`IEntityTypeConfiguration<T>` classes live in `{Service}.Infrastructure/Persistence/Configurations/`. The `DbContext` also lives in Infrastructure (at `{Service}.Infrastructure/Persistence/{Service}DbContext.cs`) and loads configurations via `modelBuilder.ApplyConfigurationsFromAssembly(typeof({Service}InfrastructureAssemblyMarker).Assembly)`.

**Rationale:** Configurations are runtime persistence concerns that belong alongside the entities they describe. Both the `DbContext` and its configurations are Infrastructure concerns — keeping them together avoids cross-project coupling and circular dependencies.

**Folder:** `{Service}.Infrastructure/Persistence/Configurations/`

```csharp
// FootballCatch.Catalog.Infrastructure/Persistence/Configurations/CompetitionEntityConfiguration.cs
using FootballCatch.Catalog.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FootballCatch.Catalog.Infrastructure.Persistence.Configurations;

public sealed class CompetitionEntityConfiguration : IEntityTypeConfiguration<CompetitionEntity>
{
    public void Configure(EntityTypeBuilder<CompetitionEntity> builder)
    {
        builder.ToTable("competitions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Country).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Season).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.ExternalId).IsUnique();
    }
}
```

The `DbContext` in `{Service}.Infrastructure` loads these configurations:

```csharp
// FootballCatch.Catalog.Infrastructure/Persistence/CatalogDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(CompetitionEntityConfiguration).Assembly);
}
```

`{Service}.Migrations` has **no** project reference to `{Service}.Infrastructure`. Schema migrations are defined independently as FluentMigrator classes in `{Service}.Migrations` — see `docs/process/migrations.md`.

---

#### Mappers defined in Infrastructure

Explicit mapper classes translate between the domain aggregate and its EF entity. No AutoMapper or reflection-based mapping tools — hand-written mappers only.

**Folder:** `{Service}.Infrastructure/Persistence/Mappers/`  
**Class name:** `{Aggregate}Mapper`

Two static methods per mapper:
- `ToEntity({Aggregate} aggregate) → {Aggregate}Entity` — for insert/update paths (domain → EF).
- `ToDomain({Aggregate}Entity entity) → {Aggregate}` — for read paths (EF → domain), calls the aggregate's `Rehydrate` factory method.

**Rationale:** Explicit mappers make the translation contract visible and testable. They keep the domain aggregate free of persistence knowledge. `Rehydrate` on the aggregate root is the designated re-constitution path; it does not raise domain events and does not enforce creation invariants, which is correct for data loaded from the database.

```csharp
// FootballCatch.Catalog.Infrastructure/Persistence/Mappers/CompetitionMapper.cs
using FootballCatch.Catalog.Domain.Aggregates;
using FootballCatch.Catalog.Domain.ValueObjects;
using FootballCatch.Catalog.Infrastructure.Persistence.Entities;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Infrastructure.Persistence.Mappers;

internal static class CompetitionMapper
{
    internal static CompetitionEntity ToEntity(Competition competition) =>
        new()
        {
            Id = competition.Id.Value,
            Name = competition.Name.Value,
            Country = competition.Country.Value,
            Season = competition.Season.Value,
            LogoUrl = competition.LogoUrl,
            ExternalId = competition.ExternalId,
            IsActive = competition.IsActive,
            CreatedAtUtc = competition.CreatedAtUtc,
            UpdatedAtUtc = competition.UpdatedAtUtc
        };

    internal static Competition ToDomain(CompetitionEntity entity) =>
        Competition.Rehydrate(
            id: new CompetitionId(entity.Id),
            name: new CompetitionName(entity.Name),
            country: new Country(entity.Country),
            season: new Season(entity.Season),
            logoUrl: entity.LogoUrl,
            externalId: entity.ExternalId,
            isActive: entity.IsActive,
            createdAtUtc: entity.CreatedAtUtc,
            updatedAtUtc: entity.UpdatedAtUtc);
}
```

Repository implementations call the mapper directly:

```csharp
// In CompetitionRepository.AddAsync
var entity = CompetitionMapper.ToEntity(competition);
await _dbContext.Competitions.AddAsync(entity, ct);

// In CompetitionRepository.GetByIdAsync
var entity = await _dbContext.Competitions.FindAsync([id.Value], ct);
return entity is null ? null : CompetitionMapper.ToDomain(entity);
```

---

#### Summary: folder structure for Infrastructure persistence

```
{Service}.Infrastructure/
  Persistence/
    Configurations/
      {Aggregate}EntityConfiguration.cs   ← IEntityTypeConfiguration<{Aggregate}Entity>
    Entities/
      {Aggregate}Entity.cs                ← Plain EF entity class; no domain types
    Mappers/
      {Aggregate}Mapper.cs                ← Static ToEntity / ToDomain methods
    Repositories/
      {Aggregate}Repository.cs            ← Implements I{Aggregate}Repository from Domain
    {Service}DbContext.cs                 ← EF Core DbContext; loads configs via ApplyConfigurationsFromAssembly

{Service}.Migrations/                     ← FluentMigrator Migration classes only; no Infrastructure reference
```

## TypeScript / Next.js (web & backoffice)
- Use ESLint + Prettier with project rules.  
- App Router (Next.js 14+): folder structure follows the `app/` convention.  
- Prefer **Server Components** by default; only use `'use client'` when strictly necessary (interactivity, browser APIs).  
- Folder by feature inside `app/` and `components/`.  
- Use hooks (`useSomething`) instead of HOCs where possible.  
- Use functional components only.  
- Keep components small and composable.

## TypeScript / React Native (app)
- Use ESLint + Prettier with project rules.  
- Expo managed workflow.  
- Folder by feature inside `screens/` and `components/`.  
- Use functional components and hooks only.

## TypeScript / Common library (`@footballcatch/common`)
- **Domain and Application layers**: zero dependencies on React, Next.js, or React Native.  
- **Infrastructure layer**: may depend on fetch/axios for HTTP and platform-agnostic storage interfaces.  
- Export everything from a single entry point (`src/index.ts`).  
- Versioned with SemVer; breaking changes require a major bump.

## Testing
- NUnit for C#.  
- Jest + React Testing Library for Next.js and React Native.  
- Playwright for E2E on web/backoffice; Detox for E2E on mobile.  
- Tests colocated in `/tests` folder with clear naming.

