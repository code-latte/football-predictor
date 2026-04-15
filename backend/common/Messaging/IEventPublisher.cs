using FootballCatch.Common.Contracts;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Publishes integration events to the message bus (e.g. RabbitMQ).
/// Concrete implementations live in each microservice's Infrastructure layer — never here.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an integration event to the message bus asynchronously.
    /// </summary>
    /// <typeparam name="T">The concrete integration event type.</typeparam>
    /// <param name="event">The event payload to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IIntegrationEvent;
}
