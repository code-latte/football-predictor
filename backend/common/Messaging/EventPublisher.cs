using FootballCatch.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Default in-process integration event publisher.
/// Resolves all registered <see cref="IIntegrationEventHandler{T}"/> instances from the
/// <see cref="IServiceProvider"/> at publish time and invokes them sequentially in
/// registration order.
/// </summary>
/// <remarks>
/// Register the publisher once per module and let modules add their own handlers:
/// <code>
/// services.AddScoped&lt;IEventPublisher, EventPublisher&gt;();
/// services.AddScoped&lt;IIntegrationEventHandler&lt;PredictionSubmittedV1&gt;, NotifyOnPredictionSubmitted&gt;();
/// </code>
/// Zero registered handlers is a valid state in a monolith — an integration event with no
/// in-process subscribers is published as a no-op.
/// </remarks>
public sealed class EventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public EventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IIntegrationEvent
    {
        IEnumerable<IIntegrationEventHandler<T>> handlers =
            _serviceProvider.GetServices<IIntegrationEventHandler<T>>();

        foreach (IIntegrationEventHandler<T> handler in handlers)
        {
            await handler.HandleAsync(@event, ct);
        }
    }
}
