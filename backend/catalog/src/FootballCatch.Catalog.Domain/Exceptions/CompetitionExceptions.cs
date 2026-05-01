namespace FootballCatch.Catalog.Domain.Exceptions;

/// <summary>Thrown when a competition name fails validation (empty or exceeds 200 characters).</summary>
public sealed class InvalidCompetitionNameException : CatalogDomainException
{
    public InvalidCompetitionNameException(string name)
        : base($"Competition name '{name}' is invalid. It must be non-empty and at most 200 characters.")
    {
    }
}

/// <summary>Thrown when an ISO country code does not conform to the 2-letter uppercase ASCII format.</summary>
public sealed class InvalidCountryCodeException : CatalogDomainException
{
    public InvalidCountryCodeException(string isoCode)
        : base($"Country ISO code '{isoCode}' is invalid. It must be exactly 2 uppercase ASCII letters (e.g. 'ES', 'GB').")
    {
    }
}

/// <summary>Thrown when a season's start year is greater than its end year.</summary>
public sealed class InvalidSeasonException : CatalogDomainException
{
    public InvalidSeasonException(int startYear, int endYear)
        : base($"Season start year {startYear} must be less than or equal to end year {endYear}.")
    {
    }
}

/// <summary>Thrown when <c>Deactivate()</c> is called on a competition that is already inactive.</summary>
public sealed class CompetitionAlreadyDeactivatedException : CatalogDomainException
{
    public CompetitionAlreadyDeactivatedException(Guid competitionId)
        : base($"Competition '{competitionId}' is already deactivated.")
    {
    }
}

/// <summary>Thrown when a mutating operation is attempted on an inactive competition.</summary>
public sealed class CompetitionInactiveException : CatalogDomainException
{
    public CompetitionInactiveException(Guid competitionId)
        : base($"Competition '{competitionId}' is inactive and cannot be modified. Reactivate it first.")
    {
    }
}
