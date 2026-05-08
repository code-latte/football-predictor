using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Catalog.Domain.ValueObjects;

/// <summary>
/// Represents a football season defined by a start and end calendar year.
/// The start year must be less than or equal to the end year.
/// Seasons that span a single calendar year have equal start and end years (e.g. 2024/2024).
/// Cross-year seasons follow the convention where EndYear = StartYear + 1 (e.g. 2023/2024).
/// </summary>
public sealed record Season(int StartYear, int EndYear) : ValueObject
{
    /// <summary>The season start year.</summary>
    public int StartYear { get; } = Validate(StartYear, EndYear);

    /// <summary>The season end year.</summary>
    public int EndYear { get; } = EndYear;

    /// <summary>A human-readable label, e.g. "2023/2024" or "2024".</summary>
    public string Label => StartYear == EndYear
        ? StartYear.ToString()
        : $"{StartYear}/{EndYear}";

    private static int Validate(int startYear, int endYear)
    {
        if (startYear > endYear)
        {
            throw new InvalidSeasonException(startYear, endYear);
        }

        return startYear;
    }
}
