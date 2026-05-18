# FootballCatch.Common

**Common** project for the backend.  
Contains definitions **shared across modules** that are **stable, timeless, and free from domain logic** specific to any module.

---

## What it contains

### 1. **Integration events (Contracts)**
- Definitions of events published/consumed between modules in-process via `IEventPublisher`.
- Versioned with suffix (`.v1`, `.v2`, …) for compatibility.
- Flat structures (DTOs) in C#.
- Example:

```csharp
public sealed record LeagueCreatedV1(
    Guid LeagueId,
    string Name,
    Guid OwnerUserId,
    string Privacy,
    DateTime OccurredAtUtc
) : IIntegrationEvent
{
    public string EventName => "league.created.v1";
}
```

---

### 2. **Messaging infrastructure**
- **Event dispatcher and publisher** (`IEventDispatcher`, `IEventPublisher`) → interfaces and their **single shared concrete implementations** in this project (`EventDispatcher`, `EventPublisher`). Follows the same pattern as `Mediator.cs`.
- **Handler interfaces** (`IDomainEventHandler<T>`, `IIntegrationEventHandler<T>`) — modules implement these and register them in DI; modules never implement the dispatcher or publisher themselves.

Example:

```csharp
public interface IIntegrationEvent
{
    DateTime OccurredAtUtc { get; }
    string EventName { get; }
}

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IIntegrationEvent;
}
```

---

### 3. **Common types and enums**
- Types reused across services (e.g. `UserId`, `LeagueId` as Strongly Typed IDs).
- Enumerations that don't belong to a specific domain but are used across several (e.g. `PrivacyLevel`, `MembershipRole`).

Example:

```csharp
public readonly record struct UserId(Guid Value);
public readonly record struct LeagueId(Guid Value);

public enum MembershipRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}
```

---

### 4. **Cross-cutting utilities**
- **Base ValueObject** (structural equality).
- **Base AggregateRoot** (domain event management).
- **Result / Guard** helpers.
- **IClock** to abstract time (facilitate testing).

---

## What should **NOT** go here

❌ Business logic of a module.  
❌ Concrete repositories or EF DbContexts.  
❌ Infrastructure-specific *integrations* (e.g. a broker-backed `IEventPublisher`, an EF `DbContext`, a Redis client) — these belong in a module's *Infrastructure* layer, not here. The in-memory `EventDispatcher` and `EventPublisher` *do* belong here because they have no infrastructure dependency.  
❌ Endpoint configuration, Docker, or specific infrastructure.  

---

## Project organisation

```
/backend/common/
  Contracts/           # Integration events (versioned)
  Messaging/           # Dispatcher and publisher interfaces, their single shared in-memory implementations, and handler interfaces (IDomainEventHandler<T>, IIntegrationEventHandler<T>)
  Types/               # Common ValueObjects, IDs, enums
  BuildingBlocks/      # Result, Guard, Entity, AggregateRoot
```

---

## Dependencies

- `FootballCatch.Common` is referenced from modules that:
  - Publish or consume events.
  - Need common types/IDs/enums.
- Versioned with **SemVer**. Breaking changes = major bump.

---
