namespace FootballCatch.Common.Mediator;

/// <summary>
/// Handles a fire-and-forget command of type <typeparamref name="TCommand"/>.
/// Implementations are registered with the DI container and resolved by <see cref="IMediator"/>.
/// </summary>
/// <typeparam name="TCommand">The command type this handler processes.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>Handles the given command.</summary>
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Handles a command of type <typeparamref name="TCommand"/> and returns a result
/// of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TCommand">The command type this handler processes.</typeparam>
/// <typeparam name="TResult">The type of the value produced by this handler.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>Handles the given command and returns a result.</summary>
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}
