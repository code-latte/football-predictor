using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FootballCatch.Common.UnitTests.Messaging;

[TestFixture]
public sealed class EventDispatcherTests
{
    [Test]
    public async Task DispatchAsync_RoutesEachEventToHandlersOfItsConcreteType()
    {
        // Arrange
        IDomainEventHandler<FakeDomainEventA> handlerA =
            Substitute.For<IDomainEventHandler<FakeDomainEventA>>();
        IDomainEventHandler<FakeDomainEventB> handlerB =
            Substitute.For<IDomainEventHandler<FakeDomainEventB>>();

        ServiceCollection services = new();
        services.AddSingleton(handlerA);
        services.AddSingleton(handlerB);
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventDispatcher sut = provider.GetRequiredService<IEventDispatcher>();
        FakeDomainEventA eventA = new(DateTime.UtcNow);
        FakeDomainEventB eventB = new(DateTime.UtcNow);

        // Act
        await sut.DispatchAsync(new IDomainEvent[] { eventA, eventB });

        // Assert — each handler saw only its own event type.
        await handlerA.Received(1).HandleAsync(eventA, Arg.Any<CancellationToken>());
        await handlerA.DidNotReceive().HandleAsync(Arg.Is<FakeDomainEventA>(e => e != eventA), Arg.Any<CancellationToken>());
        await handlerB.Received(1).HandleAsync(eventB, Arg.Any<CancellationToken>());
        await handlerB.DidNotReceive().HandleAsync(Arg.Is<FakeDomainEventB>(e => e != eventB), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DispatchAsync_FansOutToMultipleHandlersOfTheSameType()
    {
        // Arrange
        IDomainEventHandler<FakeDomainEventA> handler1 =
            Substitute.For<IDomainEventHandler<FakeDomainEventA>>();
        IDomainEventHandler<FakeDomainEventA> handler2 =
            Substitute.For<IDomainEventHandler<FakeDomainEventA>>();

        ServiceCollection services = new();
        services.AddSingleton(handler1);
        services.AddSingleton(handler2);
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventDispatcher sut = provider.GetRequiredService<IEventDispatcher>();
        FakeDomainEventA @event = new(DateTime.UtcNow);

        // Act
        await sut.DispatchAsync(new IDomainEvent[] { @event });

        // Assert
        await handler1.Received(1).HandleAsync(@event, Arg.Any<CancellationToken>());
        await handler2.Received(1).HandleAsync(@event, Arg.Any<CancellationToken>());
    }

    [Test]
    public void DispatchAsync_WithNoHandlersForEventType_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventDispatcher sut = provider.GetRequiredService<IEventDispatcher>();
        FakeDomainEventA @event = new(DateTime.UtcNow);

        // Act + Assert
        Assert.DoesNotThrowAsync(async () =>
            await sut.DispatchAsync(new IDomainEvent[] { @event }));
    }

    [Test]
    public async Task DispatchAsync_CalledTwiceWithSameEventType_StillRoutesCorrectly()
    {
        // Arrange — exercises the MethodInfo cache across calls.
        IDomainEventHandler<FakeDomainEventA> handler =
            Substitute.For<IDomainEventHandler<FakeDomainEventA>>();

        ServiceCollection services = new();
        services.AddSingleton(handler);
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventDispatcher sut = provider.GetRequiredService<IEventDispatcher>();
        FakeDomainEventA firstEvent = new(DateTime.UtcNow);
        FakeDomainEventA secondEvent = new(DateTime.UtcNow.AddSeconds(1));

        // Act
        await sut.DispatchAsync(new IDomainEvent[] { firstEvent });
        await sut.DispatchAsync(new IDomainEvent[] { secondEvent });

        // Assert
        await handler.Received(1).HandleAsync(firstEvent, Arg.Any<CancellationToken>());
        await handler.Received(1).HandleAsync(secondEvent, Arg.Any<CancellationToken>());
    }

    [Test]
    public void DispatchAsync_WithEmptyEventList_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventDispatcher sut = provider.GetRequiredService<IEventDispatcher>();

        // Act + Assert
        Assert.DoesNotThrowAsync(async () =>
            await sut.DispatchAsync(Array.Empty<IDomainEvent>()));
    }
}

public sealed record FakeDomainEventA(DateTime OccurredAtUtc) : IDomainEvent;

public sealed record FakeDomainEventB(DateTime OccurredAtUtc) : IDomainEvent;
