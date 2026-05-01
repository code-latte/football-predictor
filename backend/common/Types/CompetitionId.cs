namespace FootballCatch.Common.Types;

/// <summary>
/// Strongly-typed identifier for a competition in the Catalog bounded context.
/// Used by Fixtures and other services that reference competitions.
/// </summary>
public readonly record struct CompetitionId(Guid Value)
{
    /// <summary>Creates a new <see cref="CompetitionId"/> with a freshly generated <see cref="Guid"/>.</summary>
    public static CompetitionId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="CompetitionId"/> from an existing <see cref="Guid"/> value.</summary>
    public static CompetitionId From(Guid value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
