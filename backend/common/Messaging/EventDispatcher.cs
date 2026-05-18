using System.Collections.Concurrent;
using System.Reflection;
using FootballCatch.Common.BuildingBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace FootballCatch.Common.Messaging;

/// <summary>
/// Default in-process domain event dispatcher.
/// Resolves all registered <see cref="IDomainEventHandler{T}"/> instances for each event's
/// concrete runtime type and invokes them sequentially in registration order.
/// </summary>
/// <remarks>
/// Register the dispatcher once per module and let modules add their own handlers:
/// <code>
/// services.AddScoped&lt;IEventDispatcher, EventDispatcher&gt;();
/// services.AddScoped&lt;IDomainEventHandler&lt;PredictionSubmitted&gt;, NotifyOnPredictionSubmitted&gt;();
/// </code>
/// Reflection is required here because <c>DispatchAsync</c> receives a list typed as
/// <see cref="IDomainEvent"/> — concrete event types are only known at runtime — but each
/// closed <c>HandleAsync</c> <see cref="MethodInfo"/> is cached after the first lookup.
/// </remarks>
public sealed class EventDispatcher : IEventDispatcher
{
    // Cache keyed by concrete event type — the cached MethodInfo is on the *closed* generic
    // interface IDomainEventHandler<TConcrete>, never on the open IDomainEventHandler<>.
    private static readonly ConcurrentDictionary<Type, MethodInfo> _handlerMethodCache = new();

    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task DispatchAsync(
        IReadOnlyList<IDomainEvent> domainEvents,
        CancellationToken ct = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            Type concreteEventType = domainEvent.GetType();
            Type handlerInterface = typeof(IDomainEventHandler<>).MakeGenericType(concreteEventType);

            MethodInfo handleMethod = _handlerMethodCache.GetOrAdd(
                concreteEventType,
                _ => handlerInterface.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!);

            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerInterface);

            foreach (object? handler in handlers)
            {
                if (handler is null)
                {
                    continue;
                }

                Task task = (Task)handleMethod.Invoke(handler, new object[] { domainEvent, ct })!;
                await task;
            }
        }
    }
}
