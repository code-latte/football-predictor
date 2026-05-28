using FluentAssertions;
using FootballCatch.Auth.Application.Users.Commands.OtpLogin.VerifyOtpCode;
using FootballCatch.Auth.Domain.Exceptions;
using FootballCatch.Auth.Domain.Users;
using FootballCatch.Auth.Domain.Users.Aggregates;
using FootballCatch.Auth.Domain.Users.Repository;
using FootballCatch.Auth.Domain.Users.Services;
using FootballCatch.Common.BuildingBlocks.Persistance;
using FootballCatch.Common.Types;
using NSubstitute;
using NUnit.Framework;

namespace FootballCatch.Auth.UnitTests.Application;

[TestFixture]
public sealed class VerifyOtpCodeCommandHandlerTests
{
    private IUserRepository _userRepository;
    private IHashService _hashService;
    private IJwtTokenGenerator _jwtTokenGenerator;
    private IUnitOfWork _unitOfWork;
    private IClock _clock;
    private VerifyOtpCodeCommandHandler _handler;
    private DateTime _now;

    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _hashService = Substitute.For<IHashService>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _clock = Substitute.For<IClock>();

        _now = DateTime.UtcNow;
        _clock.UtcNow.Returns(_now);
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>()).Returns("jwt-token");

        _handler = new VerifyOtpCodeCommandHandler(
            _userRepository,
            _hashService,
            _clock,
            _jwtTokenGenerator,
            _unitOfWork
        );
    }

    private static VerifyOtpCodeCommand BuildCommand() => new(
        Email: "test@gmail.com",
        Code: "123456",
        DeviceIdentifier: "device-uuid",
        DeviceName: "Chrome Windows",
        Platform: "Web",
        Ip: "127.0.0.1"
    );

    private User BuildExistingUser(VerifyOtpCodeCommand command) =>
        User.CreateUserAndLinkToExternalProvider(
            command.Email,
            "Test User",
            "OTP",
            command.Email,
            _now
        );

    private void SetupValidOtp(string email)
    {
        _hashService.Hash("123456").Returns("hashed-code");
        var otp = OtpCode.Create("123456", _hashService,_clock.UtcNow, minutesTtl: 10);
        _hashService.Verify("123456", "hashed-code").Returns(true);
        _userRepository.GetHashedCodeAsync(email).Returns(otp);
    }

    private void SetupWrongCodeOtp(string email)
    {
        _hashService.Hash("123456").Returns("hashed-code");
        var otp = OtpCode.Create("123456", _hashService,_clock.UtcNow, minutesTtl: 10);
        _hashService.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        _userRepository.GetHashedCodeAsync(email).Returns(otp);
    }

    private void SetupExpiredOtp(string email)
    {
        _hashService.Hash("123456").Returns("hashed-code");
        var otp = OtpCode.Create("123456", _hashService,_clock.UtcNow, minutesTtl: -1);
        _userRepository.GetHashedCodeAsync(email).Returns(otp);
    }

    // -------------------------
    // Escenario 1 — Código inválido
    // -------------------------

    [Test]
    public async Task Handle_ShouldThrowInvalidOtpCodeException_WhenCodeIsWrong()
    {
        var command = BuildCommand();
        SetupWrongCodeOtp(command.Email);

        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOtpCodeException>();
    }

    [Test]
    public async Task Handle_ShouldThrowOtpCodeExpiredException_WhenCodeIsExpired()
    {
        var command = BuildCommand();
        SetupExpiredOtp(command.Email);

        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<OtpCodeExpiredException>();
    }

    [Test]
    public async Task Handle_ShouldNotSaveChanges_WhenCodeIsWrong()
    {
        var command = BuildCommand();
        SetupWrongCodeOtp(command.Email);

        try { await _handler.HandleAsync(command, CancellationToken.None); } catch { }

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldNotSaveChanges_WhenCodeIsExpired()
    {
        var command = BuildCommand();
        SetupExpiredOtp(command.Email);

        try { await _handler.HandleAsync(command, CancellationToken.None); } catch { }

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -------------------------
    // Escenario 2 — Código válido, usuario existente
    // -------------------------

    [Test]
    public async Task Handle_ShouldReturnIsNotNewUser_WhenUserExists()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(BuildExistingUser(command));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsNewUser.Should().BeFalse();
    }

    [Test]
    public async Task Handle_ShouldReturnAccessToken_WhenUserExists()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(BuildExistingUser(command));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.AccessToken.Should().Be("jwt-token");
    }

    [Test]
    public async Task Handle_ShouldReturnRefreshToken_WhenUserExists()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(BuildExistingUser(command));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Handle_ShouldRegisterDevice_WhenUserExists()
    {
        var command = BuildCommand();
        var existingUser = BuildExistingUser(command);
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        await _handler.HandleAsync(command, CancellationToken.None);

        existingUser.DeviceTokens.Should().HaveCount(1);
        existingUser.DeviceTokens.First().DeviceIdentifier.Should().Be(command.DeviceIdentifier);
    }

    [Test]
    public async Task Handle_ShouldSaveChanges_WhenUserExists()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(BuildExistingUser(command));

        await _handler.HandleAsync(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -------------------------
    // Escenario 3 — Código válido, usuario nuevo
    // -------------------------

    [Test]
    public async Task Handle_ShouldReturnIsNewUser_WhenUserDoesNotExist()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsNewUser.Should().BeTrue();
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyAccessToken_WhenUserDoesNotExist()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.AccessToken.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyRefreshToken_WhenUserDoesNotExist()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.RefreshToken.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_ShouldNotSaveChanges_WhenUserDoesNotExist()
    {
        var command = BuildCommand();
        SetupValidOtp(command.Email);
        _userRepository.FindByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        await _handler.HandleAsync(command, CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}