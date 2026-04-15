# FootballCatch.Common

**Common** project for the backend.  
Contains definitions **shared across microservices** that are **stable, timeless, and free from domain logic** specific to any service.

---

## What it contains

### 1. **Integration events (Contracts)**
- Definitions of events published/consumed between microservices.
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
- **Event dispatcher** (`IEventPublisher`, `IEventDispatcher`) → common interface and base.  
- Concrete implementations (e.g. RabbitMQ, MassTransit) are done inside each microservice's *Infrastructure* layer, **not here**.
- This project only defines the **interfaces and contracts**.

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

❌ Business logic of a microservice.  
❌ Concrete repositories or EF DbContexts.  
❌ RabbitMQ, Postgres, Redis implementations.  
❌ Endpoint configuration, Docker, or specific infrastructure.  

---

## Project organisation

```
/backend/common/
  Contracts/           # Integration events (versioned)
  Messaging/           # Dispatcher and publisher interfaces
  Types/               # Common ValueObjects, IDs, enums
  BuildingBlocks/      # Result, Guard, Entity, AggregateRoot
```

---

## Dependencies

- `FootballCatch.Common` is referenced from microservices that:
  - Publish or consume events.
  - Need common types/IDs/enums.
- Versioned with **SemVer**. Breaking changes = major bump.

---
