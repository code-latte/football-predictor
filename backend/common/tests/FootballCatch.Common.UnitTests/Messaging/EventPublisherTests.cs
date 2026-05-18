using FootballCatch.Common.Contracts;
using FootballCatch.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FootballCatch.Common.UnitTests.Messaging;

[TestFixture]
public sealed class EventPublisherTests
{
    [Test]
    public async Task PublishAsync_WithMultipleRegisteredHandlers_InvokesEachExactlyOnceWithTheSameEvent()
    {
        // Arrange
        IIntegrationEventHandler<FakeIntegrationEventV1> handlerA =
            Substitute.For<IIntegrationEventHandler<FakeIntegrationEventV1>>();
        IIntegrationEventHandler<FakeIntegrationEventV1> handlerB =
            Substitute.For<IIntegrationEventHandler<FakeIntegrationEventV1>>();

        ServiceCollection services = new();
        services.AddSingleton(handlerA);
        services.AddSingleton(handlerB);
        services.AddSingleton<IEventPublisher, EventPublisher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventPublisher sut = provider.GetRequiredService<IEventPublisher>();
        FakeIntegrationEventV1 @event = new(DateTime.UtcNow);

        // Act
        await sut.PublishAsync(@event);

        // Assert
        await handlerA.Received(1).HandleAsync(@event, Arg.Any<CancellationToken>());
        await handlerB.Received(1).HandleAsync(@event, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task PublishAsync_WithNoHandlersRegistered_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventPublisher sut = provider.GetRequiredService<IEventPublisher>();
        FakeIntegrationEventV1 @event = new(DateTime.UtcNow);

        // Act + Assert
        Assert.DoesNotThrowAsync(async () => await sut.PublishAsync(@event));
        await Task.CompletedTask;
    }

    [Test]
    public async Task PublishAsync_InvokesHandlersSequentially_NotInParallel()
    {
        // Arrange — handler B should not start until handler A's awaited work has completed.
        RecordingHandler handlerA = new(delayMs: 30);
        RecordingHandler handlerB = new(delayMs: 0);

        ServiceCollection services = new();
        services.AddSingleton<IIntegrationEventHandler<FakeIntegrationEventV1>>(handlerA);
        services.AddSingleton<IIntegrationEventHandler<FakeIntegrationEventV1>>(handlerB);
        services.AddSingleton<IEventPublisher, EventPublisher>();
        ServiceProvider provider = services.BuildServiceProvider();

        IEventPublisher sut = provider.GetRequiredService<IEventPublisher>();
        FakeIntegrationEventV1 @event = new(DateTime.UtcNow);

        // Act
        await sut.PublishAsync(@event);

        // Assert — B's start must come after A's completion.
        Assert.That(handlerA.CompletedAtTicks, Is.GreaterThan(0));
        Assert.That(handlerB.StartedAtTicks, Is.GreaterThanOrEqualTo(handlerA.CompletedAtTicks));
    }

    private sealed class RecordingHandler : IIntegrationEventHandler<FakeIntegrationEventV1>
    {
        private readonly int _delayMs;

        public RecordingHandler(int delayMs)
        {
            _delayMs = delayMs;
        }

        public long StartedAtTicks { get; private set; }

        public long CompletedAtTicks { get; private set; }

        public async Task HandleAsync(FakeIntegrationEventV1 @event, CancellationToken ct = default)
        {
            StartedAtTicks = DateTime.UtcNow.Ticks;
            if (_delayMs > 0)
            {
                await Task.Delay(_delayMs, ct);
            }
            CompletedAtTicks = DateTime.UtcNow.Ticks;
        }
    }
}

public sealed record FakeIntegrationEventV1(DateTime OccurredAtUtc) : IIntegrationEvent
{
    public string EventName => "fake.integration.v1";
}
