using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Catalog.Domain.ValueObjects;

/// <summary>
/// Represents the name of a football competition.
/// Must be non-empty and at most 200 characters.
/// </summary>
public sealed record CompetitionName(string Value) : ValueObject
{
    private const int MaxLength = 200;

    /// <summary>The validated competition name.</summary>
    public string Value { get; } = Validate(Value);

    private static string Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength)
        {
            throw new InvalidCompetitionNameException(value ?? string.Empty);
        }

        return value.Trim();
    }
}
