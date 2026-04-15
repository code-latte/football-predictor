namespace FootballCatch.Common.Mediator;

/// <summary>
/// Marker interface for fire-and-forget commands that produce no return value.
/// Commands change state; they should never return domain data — only succeed or throw.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands that produce a result of type <typeparamref name="TResult"/>.
/// Use this when the caller needs a value back (e.g. a generated ID or a validation result).
/// </summary>
/// <typeparam name="TResult">The type produced by handling this command.</typeparam>
public interface ICommand<TResult>
{
}
