# FootballCatch.Common

Proyecto **común** para el backend.  
Contiene definiciones **compartidas entre microservicios** que son **estables, atemporales y sin lógica de dominio** específica de un servicio.

---

## Qué contiene

### 1. **Eventos de integración (Contracts)**
- Definiciones de eventos publicados/consumidos entre microservicios.
- Versionados con sufijo (`.v1`, `.v2`, …) para compatibilidad.
- Estructuras planas (DTOs) en C#.
- Ejemplo:

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

### 2. **Infraestructura de mensajería**
- **Dispatcher de eventos** (`IEventPublisher`, `IEventDispatcher`) → interfaz y base común.  
- Implementaciones concretas (ej. RabbitMQ, MassTransit) se hacen en cada microservicio dentro de *Infrastructure*, **no aquí**.
- Este proyecto solo define las **interfaces y contratos**.

Ejemplo:

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

### 3. **Tipos y enums comunes**
- Tipos reutilizados entre MS (ej. `UserId`, `LeagueId` como Strongly Typed IDs).
- Enumeraciones que no pertenecen a un dominio concreto pero sí se usan en varios (ej. `PrivacyLevel`, `MembershipRole`).

Ejemplo:

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

### 4. **Utilidades cross-cutting**
- **ValueObject base** (igualdad estructural).
- **AggregateRoot base** (gestión de eventos de dominio).
- **Result / Guard** helpers.
- **IClock** para abstraer tiempo (facilitar testing).

---

## Qué **NO** debe ir aquí

❌ Reglas de negocio de un microservicio.  
❌ Repositorios concretos o EF DbContexts.  
❌ Implementaciones de RabbitMQ, Postgres, Redis.  
❌ Configuración de endpoints, Docker o infra específica.  

---

## Organización del proyecto

```
/backend/common/
  Contracts/           # Eventos de integración (versionados)
  Messaging/           # Interfaces de dispatcher y publisher
  Types/               # ValueObjects, IDs, enums comunes
  BuildingBlocks/      # Result, Guard, Entity, AggregateRoot
```

---

## Dependencias

- `FootballCatch.Common` se referencia desde los microservicios que:
  - Publican o consumen eventos.
  - Necesitan tipos/IDs/enums comunes.
- Se versiona con **SemVer**. Cambios breaking = major bump.

---
