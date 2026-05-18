using FootballCatch.Common.Contracts;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Publishes integration events to in-process subscribers via the dispatcher pattern.
/// The concrete in-memory implementation resolves handlers from the DI container and invokes
/// them within the same process. The interface shape mirrors what a real out-of-process bus
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
