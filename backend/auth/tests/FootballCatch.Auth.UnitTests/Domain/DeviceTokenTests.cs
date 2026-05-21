

using FluentAssertions;
using FootballCatch.Auth.Domain.Users;
using FootballCatch.Auth.Domain.Users.Aggregates;
using NUnit.Framework;

namespace FootballCatch.Auth.UnitTests.Domain;

[TestFixture]
public sealed class DeviceTokenTests
{
    private User _user;
    private DateTime _now;

    [SetUp]
    public void SetUp()
    {
        _user = User.CreateUserAndLinkToExternalProvider
        (
            "test@gmail.com",
            "Test", 
            "Google", 
            "google-123",
            _now
        );
        _now = DateTime.UtcNow;
    }

    // -------------------------
    // RegisterDevice
    // -------------------------

    [Test]
    public void RegisterDevice_ShouldCreateDevice_WithCorrectData()
    {
        // Act
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);

        // Assert
        device.DeviceIdentifier.Should().Be("device-uuid");
        device.DeviceName.Should().Be("Chrome Windows");
        device.Platform.Should().Be("Web");
        device.CreatedAt.Should().Be(_now);
        device.LastSeenAt.Should().Be(_now);
    }

    [Test]
    public void RegisterDevice_ShouldGenerateId()
    {
        // Act
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);

        // Assert
        device.Id.Should().NotBeEmpty();
    }

    [Test]
    public void RegisterDevice_ShouldBeActive_WhenJustCreated()
    {
        // Act
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);

        // Assert
        device.IsActive.Should().BeTrue();
        device.RevokedAt.Should().BeNull();
    }

    [Test]
    public void RegisterDevice_ShouldAddDevice_ToUserCollection()
    {
        // Act
        _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);

        // Assert
        _user.DeviceTokens.Should().HaveCount(1);
    }

    [Test]
    public void RegisterDevice_ShouldAllowMultipleDevices_ForSameUser()
    {
        // Act
        _user.RegisterDevice("device-uuid-1", "Chrome Windows", "Web", _now);
        _user.RegisterDevice("device-uuid-2", "iPhone 14", "iOS", _now);

        // Assert
        _user.DeviceTokens.Should().HaveCount(2);
    }

    [Test]
    public void RegisterDevice_ShouldReuseDevice_WhenDeviceAlreadyExists()
    {
        // Arrange
        _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);

        // Act
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now.AddDays(1));

        // Assert
        _user.DeviceTokens.Should().HaveCount(1);
        device.DeviceIdentifier.Should().Be("device-uuid");
    }

    [Test]
    public void RegisterDevice_ShouldUpdateLastSeenAt_WhenDeviceAlreadyExists()
    {
        // Arrange
        _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);
        var laterDate = _now.AddDays(1);

        // Act
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", laterDate);

        // Assert
        device.LastSeenAt.Should().Be(laterDate);
    }

    [Test]
    public void RegisterDevice_ShouldNotUpdateCreatedAt_WhenDeviceAlreadyExists()
    {
        // Arrange
        _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);
        var laterDate = _now.AddDays(1);

        // Act
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", laterDate);

        // Assert
        // CreatedAt nunca cambia — solo LastSeenAt
        device.CreatedAt.Should().Be(_now);
    }

    // -------------------------
    // UpdateLastSeen
    // -------------------------

    [Test]
    public void UpdateLastSeen_ShouldUpdateLastSeenAt()
    {
        // Arrange
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);
        var laterDate = _now.AddHours(2);

        // Act
        device.UpdateLastSeen(laterDate);

        // Assert
        device.LastSeenAt.Should().Be(laterDate);
    }

    [Test]
    public void UpdateLastSeen_ShouldNotAffectCreatedAt()
    {
        // Arrange
        var device = _user.RegisterDevice("device-uuid", "Chrome Windows", "Web", _now);

        // Act
        device.UpdateLastSeen(_now.AddHours(2));

        // Assert
        device.CreatedAt.Should().Be(_now);
    }
}