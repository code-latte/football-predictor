using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Catalog.Domain.ValueObjects;

/// <summary>
/// Represents a short abbreviation for a football team.
/// Must be 2–5 uppercase ASCII letters only (e.g. "MCI", "MUFC", "LFC").
/// </summary>
public sealed record TeamShortCode(string Value) : ValueObject
{
    private const int MinLength = 2;
    private const int MaxLength = 5;

    /// <summary>The validated short code.</summary>
    public string Value { get; } = Validate(Value);

    private static string Validate(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < MinLength || value.Length > MaxLength)
        {
            throw new InvalidTeamShortCodeException(value ?? string.Empty);
        }

        foreach (char c in value)
        {
            if (c is < 'A' or > 'Z')
            {
                throw new InvalidTeamShortCodeException(value);
            }
        }

        return value;
    }
}
