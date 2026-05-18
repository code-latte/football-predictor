# ADR 0006: Modular Monolith with In-Process Event Dispatcher

## Status
Accepted

## Context

The platform was originally designed as a system of nine independently deployable microservices communicating asynchronously over RabbitMQ (ADR-0001) with one PostgreSQL database per service (ADR-0002). At the time, the architecture optimised for independent scaling, independent deployment, and strict isolation of bounded contexts.

In practice, only two of the nine planned services have any real code today (`auth` and `catalog`, both at skeleton level). The actual scope of the product — a football prediction platform aimed at a small-to-medium user base hosted on a single VPS — does not justify the operational cost of running and orchestrating nine deployables, nine databases, a broker, and the associated CI/CD complexity. The decoupling value of cross-process events is real, but it is being paid for in operational overhead long before any of it is needed.

Alternatives considered:

| Option | Reason rejected |
|---|---|
| **Keep nine microservices on RabbitMQ (status quo)** | Operational complexity (nine containers, nine databases, broker management, distributed tracing, eventual-consistency reasoning, Inbox/Outbox patterns) is disproportionate to the current and near-term scope. Most decoupling benefits are theoretical at this stage. |
| **Single application, no event abstraction** | Throws away the decoupling discipline already designed into the codebase. Modules would tightly couple through direct method calls, making any future split painful and erasing the bounded-context model. |
| **Modular monolith with an in-process event dispatcher (selected)** | Preserves the bounded-context model and the event-driven decoupling at the module boundary. Drops cross-process and broker operational overhead. The abstraction shape is identical to a real-bus shape, so reintroducing a real broker later is a swap of one implementation, not a re-architecture. |

The pivot was approved by the project owner.

## Decision

The backend is delivered as a **modular monolith**.

- Each of the nine bounded contexts (`auth`, `user-profile`, `catalog`, `fixtures`, `predictions`, `scoring-engine`, `leagues`, `stats`, `notifications`) remains a self-contained module with full Domain / Application / Infrastructure / API layering. Module folders, project boundaries, and DDD discipline are unchanged.
- Cross-module communication that used to happen over RabbitMQ now happens **in-process via an event dispatcher**.
- The existing `IEventPublisher<T : IIntegrationEvent>` interface in `backend/common/Messaging` is repurposed as the in-process publisher contract. The interface shape is intentionally identical to the shape a real broker publisher would have: a typed publish method accepting an `IIntegrationEvent` instance. No new abstraction name is introduced.
- `IEventDispatcher` continues to dispatch intra-aggregate domain events. Its role is unchanged.
- All event contracts continue to live in `backend/common/Contracts` and continue to be versioned (`.v1`, `.v2`, …). Versioning discipline is preserved so that the door to reintroduce a real bus later is not closed.
- **Both `IEventPublisher` and `IEventDispatcher` have exactly one shared concrete implementation in `backend/common/Messaging` (`EventPublisher` and `EventDispatcher`).** Modules consume them via DI; modules never implement these two interfaces themselves. This mirrors the precedent already set by `backend/common/Mediator/Mediator.cs`. Modules contribute *handlers* — `IDomainEventHandler<T>` and `IIntegrationEventHandler<T>` implementations registered in DI — not dispatchers.

The per-module `*.Api.csproj` host projects are retained for now. No composite host project is introduced in this change; the "modular monolith" framing is conceptual and structural for the moment.

## Consequences

**Positive:**
- Drastically lower operational complexity: one backend container, no broker to operate, no queue topology to maintain, no broker-side dead-letter handling.
- Local development is simpler: starting the platform no longer requires booting RabbitMQ; integration tests no longer need a broker container.
- Cross-module events keep their existing typed shape and versioning discipline — modules still communicate through declared events, not direct method calls.
- The migration path back to a real bus (RabbitMQ or otherwise) remains clear and is even more localised under this design: only the single shared `EventPublisher` class in `backend/common/Messaging` needs to be replaced with a broker-backed implementation (or have its DI registration swapped). Module *handler* code (`IIntegrationEventHandler<T>` implementations) is unaffected, and no event contract changes are required.

**Negative / obligations:**
- Cross-process decoupling and independent deployability are lost. The whole backend is now built, tested, and deployed as one unit.
- Idempotency semantics change. In-process, each handler is invoked once per event; the Inbox pattern is no longer applied. If a handler can fail mid-transaction in a way that requires retry, the retry strategy must be designed explicitly (deferred to follow-up).
- The discipline that previously came "for free" from cross-process boundaries (no direct DB access into another module's database, no method calls across services) is now enforced only by code review and the dispatcher contract. Any breach is silent and only caught by reviewers.
- A future bus reintroduction will require revisiting in-process assumptions (handler ordering, transactional boundaries, retries) that are trivial in-process but explicit on a real bus.

## References
- ADR-0001 — Use RabbitMQ as Event Bus (superseded by this ADR).
- ADR-0007 — Shared Database, Shared Schema.
- `backend/common/Messaging` — `IEventPublisher`, `IEventDispatcher`, `IIntegrationEvent`.
- `backend/common/Contracts` — versioned event contracts.
