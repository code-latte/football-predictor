namespace FootballCatch.Catalog.Domain.Exceptions;

/// <summary>Thrown when a team name fails validation (empty or exceeds 200 characters).</summary>
public sealed class InvalidTeamNameException : CatalogDomainException
{
    public InvalidTeamNameException(string name)
        : base($"Team name '{name}' is invalid. It must be non-empty and at most 200 characters.")
    {
    }
}

/// <summary>
/// Thrown when a team short code fails validation.
/// Valid codes are 2–5 uppercase ASCII letters (e.g. "MCI", "MUFC").
/// </summary>
public sealed class InvalidTeamShortCodeException : CatalogDomainException
{
    public InvalidTeamShortCodeException(string shortCode)
        : base($"Team short code '{shortCode}' is invalid. It must be 2–5 uppercase ASCII letters only.")
    {
    }
}
