using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Handles a single concrete domain event type raised by an aggregate root.
/// Implementations live inside the module that owns the reaction to the event and are
/// registered in that module's DI container.
/// </summary>
/// <remarks>
/// Multiple handlers may be registered for the same event type. The shared
/// <c>FootballCatch.Common.Messaging.EventDispatcher</c> invokes them sequentially
/// in registration order, awaiting each before starting the next.
/// </remarks>
/// <typeparam name="T">The concrete domain event type this handler reacts to.</typeparam>
public interface IDomainEventHandler<in T>
    where T : IDomainEvent
{
    /// <summary>
    /// Reacts to a single domain event instance.
    /// </summary>
    /// <param name="event">The domain event payload.</param>
    /// <param name="ct">Cancellation token.</param>
    Task HandleAsync(T @event, CancellationToken ct = default);
}
