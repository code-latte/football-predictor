using FootballCatch.Common.Contracts;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Handles a single concrete integration event type published by another module.
/// Implementations live inside the consuming module and are registered in that module's
/// DI container; modules never implement <see cref="IEventPublisher"/> themselves.
/// </summary>
/// <remarks>
/// Multiple handlers may be registered for the same event type. The shared
/// <c>FootballCatch.Common.Messaging.EventPublisher</c> invokes them sequentially
/// in registration order, awaiting each before starting the next.
/// </remarks>
/// <typeparam name="T">The concrete integration event type this handler reacts to.</typeparam>
public interface IIntegrationEventHandler<in T>
    where T : IIntegrationEvent
{
    /// <summary>
    /// Reacts to a single integration event instance.
    /// </summary>
    /// <param name="event">The integration event payload.</param>
    /// <param name="ct">Cancellation token.</param>
    Task HandleAsync(T @event, CancellationToken ct = default);
}
