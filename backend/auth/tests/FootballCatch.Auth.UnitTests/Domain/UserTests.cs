using FluentAssertions;
using FootballCatch.Auth.Domain.Exceptions;
using FootballCatch.Auth.Domain.users;

namespace FootballCatch.Auth.UnitTests.Domain;

[TestFixture]
public sealed class UserTests
{
    private DateTime _now;
    [SetUp]
    public void SetUp()
    {
        _now=DateTime.UtcNow;
    }
    [Test]
    public void CreateWithExternalProvider_ShouldCreateUser_WithExternalLoginLinked()
    {
        // Arrange
        var email = "test@gmail.com";
        var fullName = "Test User";
        var provider = "Google";
        var providerUserId = "google-123";

        // Act
        var user = User.CreateWithExternalProvider
        (
            email, 
            fullName, 
            provider, 
            providerUserId,
            _now
        );

        // Assert
        user.Email.Should().Be(email);
        user.FullName.Should().Be(fullName);
        user.ExternalLogins.Should().HaveCount(1);
        user.ExternalLogins.First().Provider.Should().Be(provider);
        user.ExternalLogins.First().ProviderUserId.Should().Be(providerUserId);
    }
    // -------------------------
    // JoinUserWithExternalProvider
    // -------------------------

    [Test]
    public void JoinUserWithExternalProvider_ShouldLinkProvider_WhenNotAlreadyLinked()
    {
        // Arrange
        var user = User.CreateWithExternalProvider("test@gmail.com", "Test", "Google", "google-123",_now);

        // Act
        user.JoinUserWithExternalProvider("GitHub", "github-456",_now);

        // Assert
        user.ExternalLogins.Should().HaveCount(2);
        user.ExternalLogins.Any(el => el.Provider == "GitHub").Should().BeTrue();
    }

    [Test]
    public void JoinUserWithExternalProvider_ShouldThrowDomainException_WhenProviderAlreadyLinked()
    {
        // Arrange
        var user = User.CreateWithExternalProvider("test@gmail.com", "Test", "Google", "google-123",_now);

        // Act
        var act = () => user.JoinUserWithExternalProvider("Google", "google-123",_now);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Google*");
    }

    // -------------------------
    // RegisterDevice
    // -------------------------

    [Test]
    public void RegisterDevice_ShouldCreateNewDevice_WhenDeviceNotExists()
    {
        // Arrange
        var user = User.CreateWithExternalProvider("test@gmail.com", "Test", "Google", "123",_now);
        var now = DateTime.UtcNow;

        // Act
        var device = user.RegisterDevice("device-uuid", "Chrome Windows", "Web", now);

        // Assert
        user.DeviceTokens.Should().HaveCount(1);
        device.DeviceIdentifier.Should().Be("device-uuid");
        device.CreatedAt.Should().Be(now);
    }

    [Test]
    public void RegisterDevice_ShouldReuseDevice_WhenDeviceAlreadyExists()
    {
        // Arrange
        var user = User.CreateWithExternalProvider("test@gmail.com", "Test", "Google", "123",_now);
        var now = DateTime.UtcNow;
        user.RegisterDevice("device-uuid", "Chrome Windows", "Web", now);

        // Act — mismo dispositivo, segunda llamada
        var device = user.RegisterDevice("device-uuid", "Chrome Windows", "Web", now.AddDays(1));

        // Assert — sigue habiendo solo un dispositivo
        user.DeviceTokens.Should().HaveCount(1);
    }

    [Test]
    public void RegisterDevice_ShouldUpdateLastSeenAt_WhenDeviceAlreadyExists()
    {
        // Arrange
        var user = User.CreateWithExternalProvider("test@gmail.com", "Test", "Google", "123",_now);
        var now = DateTime.UtcNow;
        user.RegisterDevice("device-uuid", "Chrome Windows", "Web", now);
        var laterDate = now.AddDays(1);

        // Act
        var device = user.RegisterDevice("device-uuid", "Chrome Windows", "Web", laterDate);

        // Assert
        device.LastSeenAt.Should().Be(laterDate);
    }
}