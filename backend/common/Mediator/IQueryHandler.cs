namespace FootballCatch.Common.Mediator;

/// <summary>
/// Handles a query of type <typeparamref name="TQuery"/> and returns
/// a result of type <typeparamref name="TResult"/>.
/// Implementations are registered with the DI container and resolved by <see cref="IMediator"/>.
/// </summary>
/// <typeparam name="TQuery">The query type this handler processes.</typeparam>
/// <typeparam name="TResult">The type of data produced by this handler.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>Handles the given query and returns the result.</summary>
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
