namespace FootballCatch.Common.Types;

/// <summary>
/// Production implementation of <see cref="IClock"/> that delegates to <c>DateTime.UtcNow</c>.
/// Register as a singleton in the DI container:
/// <code>
/// services.AddSingleton&lt;IClock, SystemClock&gt;();
/// </code>
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
