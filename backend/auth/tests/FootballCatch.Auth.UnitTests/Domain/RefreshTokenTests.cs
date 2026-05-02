using FluentAssertions;
using FootballCatch.Auth.Domain.users;
using FootballCatch.Auth.Domain.users.Aggregates;
using NUnit.Framework;

namespace FootballCatch.Auth.UnitTests.Domain;

[TestFixture]
public sealed class RefreshTokenTests
{
    private DeviceToken _device;
    private DateTime _now;

    [SetUp]
    public void SetUp()
    {
        // Necesitamos un User y un DeviceToken para poder emitir RefreshTokens
        var user = User.CreateWithExternalProvider
        (
            "test@gmail.com", 
            "Test", 
            "Google", 
            "google-123",
            _now
        );
        _device = user.RegisterDevice("device-uuid", "Chrome Windows", "Web", DateTime.UtcNow);
        _now = DateTime.UtcNow;
    }

    // -------------------------
    // IssueRefreshToken
    // -------------------------

    [Test]
    public void IssueRefreshToken_ShouldCreateToken_WithNonEmptyValue()
    {
        // Act
        var refreshToken = _device.IssueRefreshToken("127.0.0.1", DateTime.UtcNow);

        // Assert
        refreshToken.Token.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void IssueRefreshToken_ShouldAddToken_ToDeviceCollection()
    {
        // Act
        _device.IssueRefreshToken("127.0.0.1", DateTime.UtcNow);

        // Assert
        _device.RefreshTokens.Should().HaveCount(1);
    }

    [Test]
    public void IssueRefreshToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var refreshToken = _device.IssueRefreshToken("127.0.0.1", now);

        // Assert
        refreshToken.ExpiresAt.Should().BeCloseTo(now.AddDays(30), TimeSpan.FromSeconds(1));
    }

    [Test]
    public void IssueRefreshToken_ShouldSetCreatedByIp()
    {
        // Arrange
        var ip = "192.168.1.1";

        // Act
        var refreshToken = _device.IssueRefreshToken(ip, DateTime.UtcNow);

        // Assert
        refreshToken.CreatedByIp.Should().Be(ip);
    }

    [Test]
    public void IssueRefreshToken_ShouldBeActive_WhenJustCreated()
    {
        // Act
        var refreshToken = _device.IssueRefreshToken("127.0.0.1", DateTime.UtcNow);

        // Assert
        refreshToken.RevokedAt.Should().BeNull();
        refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Test]
    public void IssueRefreshToken_ShouldGenerateUniqueTokens_OnMultipleCalls()
    {
        // Act
        var token1 = _device.IssueRefreshToken("127.0.0.1", DateTime.UtcNow);
        var token2 = _device.IssueRefreshToken("127.0.0.1", DateTime.UtcNow);

        // Assert
        token1.Token.Should().NotBe(token2.Token);
        _device.RefreshTokens.Should().HaveCount(2);
    }

    [Test]
    public void IssueRefreshToken_ShouldGenerateUniqueIds()
    {
        // Act
        var token1 = _device.IssueRefreshToken("127.0.0.1", DateTime.UtcNow);
        var token2 = _device.IssueRefreshToken("127.0.0.1", DateTime.UtcNow);

        // Assert
        token1.Id.Should().NotBe(token2.Id);
    }
}