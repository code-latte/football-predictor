using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Catalog.Domain.ValueObjects;

/// <summary>
/// Represents the full name of a football team.
/// Must be non-empty and at most 200 characters.
/// </summary>
public sealed record TeamName(string Value) : ValueObject
{
    private const int MaxLength = 200;

    /// <summary>The validated team name.</summary>
    public string Value { get; } = Validate(Value);

    private static string Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength)
        {
            throw new InvalidTeamNameException(value ?? string.Empty);
        }

        return value.Trim();
    }
}
