using Microsoft.Extensions.DependencyInjection;

namespace FootballCatch.Common.Mediator;

/// <summary>
/// Default in-process mediator implementation.
/// Resolves command and query handlers from the <see cref="IServiceProvider"/> at dispatch time,
/// so handlers benefit from scoped DI lifetimes when resolved inside a request scope.
/// </summary>
/// <remarks>
/// Register handlers and the mediator in each service's <c>Program.cs</c>:
/// <code>
/// services.AddScoped&lt;ICommandHandler&lt;SubmitPredictionCommand&gt;, SubmitPredictionHandler&gt;();
/// services.AddScoped&lt;IMediator, Mediator&gt;();
/// </code>
/// </remarks>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand
    {
        ICommandHandler<TCommand> handler = ResolveHandler<ICommandHandler<TCommand>>(
            typeof(ICommandHandler<TCommand>));

        await handler.HandleAsync(command, ct);
    }

    /// <inheritdoc />
    public async Task<TResult> SendAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken ct = default)
        where TCommand : ICommand<TResult>
    {
        ICommandHandler<TCommand, TResult> handler =
            ResolveHandler<ICommandHandler<TCommand, TResult>>(
                typeof(ICommandHandler<TCommand, TResult>));

        return await handler.HandleAsync(command, ct);
    }

    /// <inheritdoc />
    public async Task<TResult> QueryAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken ct = default)
        where TQuery : IQuery<TResult>
    {
        IQueryHandler<TQuery, TResult> handler =
            ResolveHandler<IQueryHandler<TQuery, TResult>>(
                typeof(IQueryHandler<TQuery, TResult>));

        return await handler.HandleAsync(query, ct);
    }

    private THandler ResolveHandler<THandler>(Type handlerType)
    {
        object? handler = _serviceProvider.GetService(handlerType);

        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No handler registered for '{handlerType.FullName}'. " +
                "Ensure the handler is registered in the DI container.");
        }

        return (THandler)handler;
    }
}
