using FluentAssertions;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;

namespace FootballCatch.Catalog.UnitTests.ValueObjects;

[TestFixture]
public sealed class CompetitionNameTests
{
    [TestCase("Premier League")]
    [TestCase("La Liga")]
    [TestCase("A")] // single character is valid
    public void Constructor_WithValidName_ShouldCreateCompetitionName(string name)
    {
        // Act
        CompetitionName competitionName = new(name);

        // Assert
        competitionName.Value.Should().Be(name.Trim());
    }

    [Test]
    public void Constructor_WithNameExactly200Chars_ShouldSucceed()
    {
        // Arrange
        string longName = new('A', 200);

        // Act
        CompetitionName competitionName = new(longName);

        // Assert
        competitionName.Value.Should().HaveLength(200);
    }

    [Test]
    public void Constructor_WithNameExceeding200Chars_ShouldThrowInvalidCompetitionNameException()
    {
        // Arrange
        string tooLong = new('A', 201);

        // Act
        Action act = () => _ = new CompetitionName(tooLong);

        // Assert
        act.Should().Throw<InvalidCompetitionNameException>();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Constructor_WithEmptyOrWhitespaceName_ShouldThrowInvalidCompetitionNameException(string name)
    {
        // Act
        Action act = () => _ = new CompetitionName(name);

        // Assert
        act.Should().Throw<InvalidCompetitionNameException>();
    }

    [Test]
    public void Constructor_WithLeadingAndTrailingWhitespace_ShouldTrimValue()
    {
        // Act
        CompetitionName competitionName = new("  Premier League  ");

        // Assert
        competitionName.Value.Should().Be("Premier League");
    }

    [Test]
    public void TwoNamesWithSameValue_ShouldBeEqual()
    {
        // Arrange
        CompetitionName a = new("La Liga");
        CompetitionName b = new("La Liga");

        // Assert
        a.Should().Be(b);
    }
}
