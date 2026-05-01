namespace FootballCatch.Common.Types;

/// <summary>
/// Strongly-typed identifier for a team in the Catalog bounded context.
/// Used by Fixtures and other services that reference teams.
/// </summary>
public readonly record struct TeamId(Guid Value)
{
    /// <summary>Creates a new <see cref="TeamId"/> with a freshly generated <see cref="Guid"/>.</summary>
    public static TeamId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="TeamId"/> from an existing <see cref="Guid"/> value.</summary>
    public static TeamId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
