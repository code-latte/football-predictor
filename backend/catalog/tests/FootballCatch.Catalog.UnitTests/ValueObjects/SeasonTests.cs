using FluentAssertions;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;

namespace FootballCatch.Catalog.UnitTests.ValueObjects;

[TestFixture]
public sealed class SeasonTests
{
    [Test]
    public void Constructor_WithStartYearLessThanEndYear_ShouldCreateSeason()
    {
        // Act
        Season season = new(2023, 2024);

        // Assert
        season.StartYear.Should().Be(2023);
        season.EndYear.Should().Be(2024);
    }

    [Test]
    public void Constructor_WithEqualStartAndEndYear_ShouldCreateSeason()
    {
        // Act
        Season season = new(2024, 2024);

        // Assert
        season.StartYear.Should().Be(2024);
        season.EndYear.Should().Be(2024);
    }

    [Test]
    public void Constructor_WithStartYearGreaterThanEndYear_ShouldThrowInvalidSeasonException()
    {
        // Act
        Action act = () => _ = new Season(2025, 2024);

        // Assert
        act.Should().Throw<InvalidSeasonException>();
    }

    [Test]
    public void Label_ForCrossYearSeason_ShouldShowBothYears()
    {
        // Arrange
        Season season = new(2023, 2024);

        // Assert
        season.Label.Should().Be("2023/2024");
    }

    [Test]
    public void Label_ForSingleYearSeason_ShouldShowOnlyOneYear()
    {
        // Arrange
        Season season = new(2024, 2024);

        // Assert
        season.Label.Should().Be("2024");
    }

    [Test]
    public void TwoSeasonsWithSameYears_ShouldBeEqual()
    {
        // Arrange
        Season a = new(2023, 2024);
        Season b = new(2023, 2024);

        // Assert
        a.Should().Be(b);
    }
}
