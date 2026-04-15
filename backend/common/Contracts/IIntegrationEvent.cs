namespace FootballCatch.Common.Contracts;

/// <summary>
/// Marker interface for all integration events exchanged between microservices via RabbitMQ.
/// Concrete events are flat, immutable <c>sealed record</c> types that implement this interface.
/// </summary>
/// <remarks>
/// Naming convention: <c>{domain}.{noun}.{verb}.v{n}</c> — e.g. <c>prediction.submitted.v1</c>.
/// Never modify a published event's shape — add a new versioned event instead.
/// </remarks>
public interface IIntegrationEvent
{
    /// <summary>UTC timestamp recording when the event occurred in the emitting service.</summary>
    DateTime OccurredAtUtc { get; }

    /// <summary>
    /// Stable, lowercase dot-separated event name including the version suffix.
    /// Example: <c>"prediction.submitted.v1"</c>
    /// </summary>
    string EventName { get; }
}
