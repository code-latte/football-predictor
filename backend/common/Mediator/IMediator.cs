namespace FootballCatch.Common.Mediator;

/// <summary>
/// In-process mediator that routes commands and queries to their registered handlers.
/// This is a custom implementation — no MediatR dependency.
/// Handlers are resolved from the DI container at dispatch time.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a fire-and-forget command to its registered <see cref="ICommandHandler{TCommand}"/>.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand;

    /// <summary>
    /// Sends a command to its registered <see cref="ICommandHandler{TCommand, TResult}"/>
    /// and returns the result.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResult">The type produced by the handler.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Sends a query to its registered <see cref="IQueryHandler{TQuery, TResult}"/>
    /// and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <typeparam name="TResult">The type returned by the handler.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct = default)
        where TQuery : IQuery<TResult>;
}
