using FluentAssertions;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;

namespace FootballCatch.Catalog.UnitTests.ValueObjects;

[TestFixture]
public sealed class CountryTests
{
    [TestCase("ES", "Spain")]
    [TestCase("GB", "United Kingdom")]
    [TestCase("DE", "Germany")]
    public void Constructor_WithValidIsoCodeAndDisplayName_ShouldCreateCountry(string isoCode, string displayName)
    {
        // Act
        Country country = new(isoCode, displayName);

        // Assert
        country.IsoCode.Should().Be(isoCode);
        country.DisplayName.Should().Be(displayName);
    }

    [TestCase("")]
    [TestCase("E")]
    [TestCase("ESP")]
    [TestCase("es")]
    [TestCase("E1")]
    [TestCase("1A")]
    public void Constructor_WithInvalidIsoCode_ShouldThrowInvalidCountryCodeException(string isoCode)
    {
        // Act
        Action act = () => _ = new Country(isoCode, "Spain");

        // Assert
        act.Should().Throw<InvalidCountryCodeException>();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Constructor_WithEmptyDisplayName_ShouldThrowInvalidCountryCodeException(string displayName)
    {
        // Act
        Action act = () => _ = new Country("ES", displayName);

        // Assert
        act.Should().Throw<InvalidCountryCodeException>();
    }

    [Test]
    public void TwoCountriesWithSameIsoCodeAndDisplayName_ShouldBeEqual()
    {
        // Arrange
        Country a = new("ES", "Spain");
        Country b = new("ES", "Spain");

        // Assert
        a.Should().Be(b);
    }

    [Test]
    public void TwoCountriesWithDifferentIsoCode_ShouldNotBeEqual()
    {
        // Arrange
        Country a = new("ES", "Spain");
        Country b = new("DE", "Germany");

        // Assert
        a.Should().NotBe(b);
    }
}
