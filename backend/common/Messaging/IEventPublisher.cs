using FootballCatch.Common.Contracts;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Publishes integration events to in-process subscribers via the dispatcher pattern.
/// The shared concrete implementation is <c>FootballCatch.Common.Messaging.EventPublisher</c>
/// in this same project. Modules register it once in DI and register their own
/// <c>IIntegrationEventHandler&lt;T&gt;</c> implementations alongside it; modules never implement
/// <c>IEventPublisher</c> themselves. The interface shape mirrors what a real out-of-process bus
/// would expose so a real bus can be reintroduced later without changing call sites.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an integration event to all registered in-process handlers asynchronously.
    /// </summary>
    /// <typeparam name="T">The concrete integration event type.</typeparam>
    /// <param name="event">The event payload to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IIntegrationEvent;
}
