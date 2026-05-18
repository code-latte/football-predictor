using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Dispatches domain events that were collected on an aggregate root during a unit of work.
/// This is called by the infrastructure layer <em>after</em> the aggregate has been persisted,
/// so domain event handlers run in the same logical transaction window but after the state
/// change is durable.
/// </summary>
/// <remarks>
/// The shared concrete implementation is <c>FootballCatch.Common.Messaging.EventDispatcher</c>
/// in this same project. Modules register it once in DI and register their own
/// <c>IDomainEventHandler&lt;T&gt;</c> implementations alongside it; modules never implement
/// <c>IEventDispatcher</c> themselves.
/// </remarks>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches all collected domain events to their registered handlers.
    /// </summary>
    /// <param name="domainEvents">
    /// The list of domain events raised by the aggregate root, typically obtained via
    /// <c>aggregate.DomainEvents</c> before calling <c>aggregate.ClearDomainEvents()</c>.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken ct = default);
}
