namespace FootballCatch.Common.BuildingBlocks;

/// <summary>
/// Marker interface for all domain events raised within an aggregate.
/// Domain events are dispatched after persistence — never before.
/// </summary>
public interface IDomainEvent
{
    public DateTime OccurredAtUtc { get; }
}

/// <summary>
/// Base record for concrete domain events.
/// Automatically captures the UTC timestamp at the moment of construction.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
