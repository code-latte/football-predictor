namespace FootballCatch.Common.Types;

/// <summary>
/// Abstracts the system clock so that time-dependent domain logic and application services
/// can be tested deterministically without relying on <c>DateTime.UtcNow</c> directly.
/// </summary>
/// <remarks>
/// Always inject <see cref="IClock"/> rather than calling <c>DateTime.UtcNow</c> directly
/// in domain objects, aggregates, and application handlers.
/// In production, register <see cref="SystemClock"/> as the singleton implementation.
/// In tests, provide a stub that returns a controlled value.
/// </remarks>
public interface IClock
{
    /// <summary>Gets the current UTC date and time.</summary>
    DateTime UtcNow { get; }
}
