using FluentAssertions;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;

namespace FootballCatch.Catalog.UnitTests.ValueObjects;

[TestFixture]
public sealed class TeamNameTests
{
    [TestCase("Manchester City")]
    [TestCase("FC Barcelona")]
    [TestCase("A")]
    public void Constructor_WithValidName_ShouldCreateTeamName(string name)
    {
        // Act
        TeamName teamName = new(name);

        // Assert
        teamName.Value.Should().Be(name.Trim());
    }

    [Test]
    public void Constructor_WithNameExactly200Chars_ShouldSucceed()
    {
        // Arrange
        string longName = new('A', 200);

        // Act
        TeamName teamName = new(longName);

        // Assert
        teamName.Value.Should().HaveLength(200);
    }

    [Test]
    public void Constructor_WithNameExceeding200Chars_ShouldThrowInvalidTeamNameException()
    {
        // Arrange
        string tooLong = new('A', 201);

        // Act
        Action act = () => _ = new TeamName(tooLong);

        // Assert
        act.Should().Throw<InvalidTeamNameException>();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Constructor_WithEmptyOrWhitespaceName_ShouldThrowInvalidTeamNameException(string name)
    {
        // Act
        Action act = () => _ = new TeamName(name);

        // Assert
        act.Should().Throw<InvalidTeamNameException>();
    }
}
