using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Catalog.Domain.ValueObjects;

/// <summary>
/// Represents a country by its ISO 3166-1 alpha-2 code and a human-readable display name.
/// The ISO code must be exactly 2 uppercase ASCII letters (e.g. "ES", "GB", "DE").
/// </summary>
public sealed record Country(string IsoCode, string DisplayName) : ValueObject
{
    /// <summary>The validated 2-letter uppercase ISO code.</summary>
    public string IsoCode { get; } = Validate(IsoCode);

    /// <summary>Human-readable name used for display purposes.</summary>
    public string DisplayName { get; } = string.IsNullOrWhiteSpace(DisplayName)
        ? throw new InvalidCountryCodeException(DisplayName ?? string.Empty)
        : DisplayName.Trim();

    private static string Validate(string isoCode)
    {
        if (isoCode is not { Length: 2 })
        {
            throw new InvalidCountryCodeException(isoCode ?? string.Empty);
        }

        foreach (char c in isoCode)
        {
            if (c is < 'A' or > 'Z')
            {
                throw new InvalidCountryCodeException(isoCode);
            }
        }

        return isoCode;
    }
}
