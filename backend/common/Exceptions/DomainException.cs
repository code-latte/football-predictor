namespace FootballCatch.Common.Exceptions;
/// <summary>
/// Represents an error that originates from a violation of a domain rule or invariant.
/// Throw this exception when the domain model reaches an invalid or inconsistent state.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="DomainException"/> with a message describing the violated rule.
    /// </summary>
    /// <param name="message"></param>
    public DomainException(string message) : base(message){}
}

