namespace FootballCatch.Common.BuildingBlocks;

/// <summary>
/// Base class for all aggregate roots.
/// Maintains a private list of domain events raised during the lifetime of the aggregate.
/// Events are collected here and dispatched <em>after</em> the aggregate has been persisted.
/// </summary>
/// <typeparam name="TId">The strongly-typed ID type of the aggregate root.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Read-only view of all domain events raised since the last call to <see cref="ClearDomainEvents"/>.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Records a domain event to be dispatched after the aggregate is persisted.
    /// Call this from within aggregate methods that change state.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all collected domain events.
    /// Should be called by the infrastructure layer after events have been dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
