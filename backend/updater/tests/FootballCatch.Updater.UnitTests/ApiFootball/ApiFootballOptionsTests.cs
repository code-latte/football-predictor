using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using FootballCatch.Updater.Infrastructure.ApiFootball;

namespace FootballCatch.Updater.UnitTests.ApiFootball;

[TestFixture]
public sealed class ApiFootballOptionsTests
{
    private static bool TryValidate(ApiFootballOptions options, out List<ValidationResult> results)
    {
        results = new List<ValidationResult>();
        ValidationContext context = new(options);
        return Validator.TryValidateObject(options, context, results, validateAllProperties: true);
    }

    [Test]
    public void Validate_WithEmptyBaseUrl_ShouldFail()
    {
        // Arrange
        ApiFootballOptions options = new()
        {
            BaseUrl = string.Empty,
            ApiKey = "some-key"
        };

        // Act
        bool isValid = TryValidate(options, out List<ValidationResult> results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ApiFootballOptions.BaseUrl)));
    }

    [Test]
    public void Validate_WithMalformedBaseUrl_ShouldFail()
    {
        // Arrange
        ApiFootballOptions options = new()
        {
            BaseUrl = "not-a-url",
            ApiKey = "some-key"
        };

        // Act
        bool isValid = TryValidate(options, out List<ValidationResult> results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ApiFootballOptions.BaseUrl)));
    }

    [Test]
    public void Validate_WithEmptyApiKey_ShouldFail()
    {
        // Arrange
        ApiFootballOptions options = new()
        {
            BaseUrl = "https://v3.football.api-sports.io/",
            ApiKey = string.Empty
        };

        // Act
        bool isValid = TryValidate(options, out List<ValidationResult> results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(ApiFootballOptions.ApiKey)));
    }

    [Test]
    public void Validate_WithAllRequiredFieldsSet_ShouldPass()
    {
        // Arrange
        ApiFootballOptions options = new()
        {
            BaseUrl = "https://v3.football.api-sports.io/",
            ApiKey = "some-key"
        };

        // Act
        bool isValid = TryValidate(options, out List<ValidationResult> results);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Test]
    public void Defaults_ShouldHaveRetryCount3AndBaseDelay1AndEmptyActiveCompetitions()
    {
        // Arrange + Act
        ApiFootballOptions options = new();

        // Assert
        options.RetryCount.Should().Be(3);
        options.BaseDelaySeconds.Should().Be(1.0);
        options.ActiveCompetitions.Should().BeEmpty();
    }
}
