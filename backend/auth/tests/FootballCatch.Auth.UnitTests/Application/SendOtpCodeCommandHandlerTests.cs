using FluentAssertions;
using FootballCatch.Auth.Application.Users.Commands.OtpLogin.SendOtpCode;
using FootballCatch.Auth.Domain.Users.Repository;
using FootballCatch.Auth.Domain.Users.Services;
using FootballCatch.Common.BuildingBlocks.Persistance;
using FootballCatch.Common.Types;
using NSubstitute;
using NUnit.Framework;
namespace FootballCatch.Auth.UnitTests.Application;

[TestFixture]
public sealed class SendOtpCodeCommandHandlerTests
{
    private IEmailSender _emailSender;
    private IHashService _hashService;
    private IUserRepository _userRepository;
    private IUnitOfWork _unitOfWork;
    private SendOtpCodeCommandHandler _handler;
    private IClock _clock;

    [SetUp]
    public void SetUp()
    {
        _emailSender = Substitute.For<IEmailSender>();
        _hashService = Substitute.For<IHashService>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _clock = Substitute.For<IClock>();

        _hashService.Hash(Arg.Any<string>()).Returns("hashed-code");

        _handler = new SendOtpCodeCommandHandler(
            _emailSender,
            _hashService,
            _userRepository,
            _unitOfWork,
            _clock
        );
    }

    private static SendOtpCodeCommand BuildCommand() => new(
        Email: "test@gmail.com"
    );

    // -------------------------
    // Escenario 1 — Persistencia del código
    // -------------------------

    [Test]
    public async Task Handle_ShouldSaveHashedCode_WhenCommandIsValid()
    {
        var command = BuildCommand();

        await _handler.HandleAsync(command, CancellationToken.None);

        await _userRepository.Received(1).AddOtpCodeAsync("hashed-code");
    }

    [Test]
    public async Task Handle_ShouldSaveChanges_WhenCommandIsValid()
    {
        var command = BuildCommand();

        await _handler.HandleAsync(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldSaveChangesAfterPersistingOtpCode()
    {
        var command = BuildCommand();
        var callOrder = new List<string>();

        _userRepository.AddOtpCodeAsync(Arg.Any<string>())
            .Returns(_ => { callOrder.Add("save-otp"); return Task.CompletedTask; });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => { callOrder.Add("save-changes"); return Task.CompletedTask; });

        await _handler.HandleAsync(command, CancellationToken.None);

        callOrder.Should().ContainInOrder("save-otp", "save-changes");
    }

    // -------------------------
    // Escenario 2 — Envío del email
    // -------------------------

    [Test]
    public async Task Handle_ShouldSendEmail_ToCorrectRecipient()
    {
        var command = BuildCommand();

        await _handler.HandleAsync(command, CancellationToken.None);

        await _emailSender.Received(1).Send(command.Email, Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public async Task Handle_ShouldSendEmail_WithPlainCode_NotHashedCode()
    {
        var command = BuildCommand();
        string? capturedBody = null;
        _emailSender.Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Do<string>(x => capturedBody = x))
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(command, CancellationToken.None);

        capturedBody.Should().NotBe("hashed-code");
        capturedBody.Should().HaveLength(6);
        capturedBody.Should().MatchRegex(@"^\d+$");
    }

    [Test]
    public async Task Handle_ShouldSendEmailBeforeSavingChanges()
    {
        var command = BuildCommand();
        var callOrder = new List<string>();

        _emailSender.Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(_ => { callOrder.Add("send-email"); return Task.CompletedTask; });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => { callOrder.Add("save-changes"); return Task.CompletedTask; });

        await _handler.HandleAsync(command, CancellationToken.None);

        callOrder.Should().ContainInOrder("send-email", "save-changes");
    }

    // -------------------------
    // Escenario 3 — Generación del código OTP
    // -------------------------

    [Test]
    public void GenerateOtpCode_ShouldReturnSixDigits_ByDefault()
    {
        var code = _handler.GenerateOtpCode();

        code.Should().HaveLength(6);
    }

    [Test]
    public void GenerateOtpCode_ShouldReturnOnlyDigits()
    {
        var code = _handler.GenerateOtpCode();

        code.Should().MatchRegex(@"^\d+$");
    }

    [Test]
    public void GenerateOtpCode_ShouldPadWithLeadingZeros_WhenNumberIsSmall()
    {
        var codes = Enumerable.Range(0, 100).Select(_ => _handler.GenerateOtpCode()).ToList();

        codes.Should().AllSatisfy(c => c.Should().HaveLength(6));
    }

    [Test]
    public void GenerateOtpCode_ShouldReturnCustomLength_WhenLengthIsSpecified()
    {
        var code = _handler.GenerateOtpCode(length: 4);

        code.Should().HaveLength(4);
    }

    // -------------------------
    // Escenario 4 — Hashing
    // -------------------------

    [Test]
    public async Task Handle_ShouldHashOtpCode_BeforeSaving()
    {
        var command = BuildCommand();
        string? capturedPlainCode = null;
        _hashService.Hash(Arg.Do<string>(x => capturedPlainCode = x)).Returns("hashed-code");

        await _handler.HandleAsync(command, CancellationToken.None);

        _hashService.Received(1).Hash(Arg.Any<string>());
        capturedPlainCode.Should().HaveLength(6);
        capturedPlainCode.Should().MatchRegex(@"^\d+$");
    }
}