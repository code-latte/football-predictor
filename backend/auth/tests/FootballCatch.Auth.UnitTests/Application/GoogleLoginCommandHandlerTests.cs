using FluentAssertions;
using FootballCatch.Auth.Application.Users.Commands.GoogleLogin;
using FootballCatch.Auth.Domain.Users;
using FootballCatch.Auth.Domain.Users.Aggregates;
using FootballCatch.Auth.Domain.Users.Repository;
using FootballCatch.Auth.Domain.Users.Services;
using FootballCatch.Common.BuildingBlocks.Persistance;
using FootballCatch.Common.Types;
using NSubstitute;
using NUnit.Framework;

namespace FootballCatch.Auth.Tests.Application;

[TestFixture]
public sealed class GoogleLoginCommandHandlerTests
{
    private IUserRepository _userRepository;
    private IJwtTokenGenerator _jwtTokenGenerator;
    private IUnitOfWork _unitOfWork;
    private IClock _clock;
    private GoogleLoginCommandHandler _handler;
    private DateTime _now;

    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _clock = Substitute.For<IClock>();

        _now = DateTime.UtcNow;
        _clock.UtcNow.Returns(_now);
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>()).Returns("jwt-token");

        _handler = new GoogleLoginCommandHandler(
            _userRepository,
            _jwtTokenGenerator,
            _clock,
            _unitOfWork
            
        );
    }

    private static GoogleLoginCommand BuildCommand() => new(
        Provider: "Google",
        ProviderUserId: "google-123",
        Email: "test@gmail.com",
        FullName: "Test User",
        ProviderEmail: "test@gmail.com",
        DeviceName: "Chrome Windows",
        Platform: "Web",
        Ip: "127.0.0.1",
        DeviceIdentifier: "device-uuid"
    );

    // -------------------------
    // Escenario 1 — Usuario nuevo
    // -------------------------

    [Test]
    public async Task Handle_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var command = BuildCommand();
        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldReturnIsNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var command = BuildCommand();
        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsNewUser.Should().BeTrue();
    }

    [Test]
    public async Task Handle_ShouldReturnTokens_WhenUserDoesNotExist()
    {
        // Arrange
        var command = BuildCommand();
        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    // -------------------------
    // Escenario 2 — Usuario existente sin Google
    // -------------------------

    [Test]
    public async Task Handle_ShouldLinkGoogleProvider_WhenUserExistsWithEmailOnly()
    {
        // Arrange
        var command = BuildCommand();
        var existingUser = User.CreateUserAndLinkToExternalProvider
        (
            command.Email, 
            command.FullName, 
            "GitHub", 
            "github-456",
            _now
        );

        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        existingUser.ExternalLogins.Should().HaveCount(2);
        existingUser.ExternalLogins.Any(el => el.Provider == "Google").Should().BeTrue();
    }

    [Test]
    public async Task Handle_ShouldReturnIsNotNewUser_WhenUserExistsWithEmailOnly()
    {
        // Arrange
        var command = BuildCommand();
        var existingUser = User.CreateUserAndLinkToExternalProvider
        (
            command.Email, 
            command.FullName, 
            "GitHub", 
            "github-456",
            _now
        );

        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsNewUser.Should().BeFalse();
    }

    // -------------------------
    // Escenario 3 — Usuario existente con Google
    // -------------------------

    [Test]
    public async Task Handle_ShouldReuseExistingUser_WhenUserAlreadyHasGoogle()
    {
        // Arrange
        var command = BuildCommand();
        var existingUser = User.CreateUserAndLinkToExternalProvider
        (
            command.Email, 
            command.FullName, 
            command.Provider, 
            command.ProviderUserId,
            _now
        );

        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        result.IsNewUser.Should().BeFalse();
    }

    [Test]
    public async Task Handle_ShouldReturnTokens_WhenUserAlreadyHasGoogle()
    {
        // Arrange
        var command = BuildCommand();
        var existingUser = User.CreateUserAndLinkToExternalProvider
        (
            command.Email,
            command.FullName, 
            command.Provider, 
            command.ProviderUserId,
            _now
        );

        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Handle_ShouldAlwaysSaveChanges_InAllScenarios()
    {
        // Arrange
        var command = BuildCommand();
        _userRepository.FindByExternalLoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}