namespace FootballCatch.Common.Mediator;

/// <summary>
/// Marker interface for queries that return a result of type <typeparamref name="TResult"/>.
/// Queries are read-only: they must never change state or raise domain events.
/// Query handlers read directly from projections or read models — never through aggregates.
/// </summary>
/// <typeparam name="TResult">The type of data returned by this query.</typeparam>
public interface IQuery<TResult>
{
}
