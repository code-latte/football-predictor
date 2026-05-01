using FluentAssertions;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;

namespace FootballCatch.Catalog.UnitTests.ValueObjects;

[TestFixture]
public sealed class TeamShortCodeTests
{
    [TestCase("MC")]
    [TestCase("MCI")]
    [TestCase("MUFC")]
    [TestCase("THFCX")] // 5 chars — max
    public void Constructor_WithValidShortCode_ShouldCreateTeamShortCode(string code)
    {
        // Act
        TeamShortCode shortCode = new(code);

        // Assert
        shortCode.Value.Should().Be(code);
    }

    [TestCase("M")]      // too short (1 char)
    [TestCase("ABCDEF")] // too long (6 chars)
    [TestCase("mc")]     // lowercase
    [TestCase("M1")]     // digit
    [TestCase("")]       // empty
    public void Constructor_WithInvalidShortCode_ShouldThrowInvalidTeamShortCodeException(string code)
    {
        // Act
        Action act = () => _ = new TeamShortCode(code);

        // Assert
        act.Should().Throw<InvalidTeamShortCodeException>();
    }

    [Test]
    public void TwoShortCodesWithSameValue_ShouldBeEqual()
    {
        // Arrange
        TeamShortCode a = new("MCI");
        TeamShortCode b = new("MCI");

        // Assert
        a.Should().Be(b);
    }
}
