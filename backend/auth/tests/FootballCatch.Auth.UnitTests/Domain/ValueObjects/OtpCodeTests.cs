
using FluentAssertions;
using FluentAssertions.Common;
using FootballCatch.Auth.Domain.Exceptions;
using FootballCatch.Auth.Domain.Users.Aggregates;
using FootballCatch.Auth.Domain.Users.Services;
using NSubstitute;
using NUnit.Framework;

namespace FootballCatch.Auth.UnitTests.Domain.ValueObjects;

[TestFixture]
public sealed class OtpCodeTests
{
    private IHashService _hashService;
    private DateTime _now;

    [SetUp]
    public void SetUp()
    {
        _hashService = Substitute.For<IHashService>();
        _now = DateTime.UtcNow;
        _hashService.Hash(Arg.Any<string>()).Returns("hashed-code");
    }

    private OtpCode BuildValidOtpCode()
    {
        var otp = OtpCode.Create("123456", _hashService,_now, minutesTtl: 10);
        _hashService.Verify("123456", "hashed-code").Returns(true);
        return otp;
    }

    private OtpCode BuildExpiredOtpCode()
    {
        return OtpCode.Create("123456", _hashService,_now, minutesTtl: -1);
    }

    private OtpCode BuildUsedOtpCode()
    {
        var otp = OtpCode.Create("123456", _hashService,_now, minutesTtl: 10);
        otp.MarkAsUsed();
        return otp;
    }

    // -------------------------
    // Create
    // -------------------------

    [Test]
    public void Create_ShouldHashPlainCode()
    {
        OtpCode.Create("123456", _hashService,_now);

        _hashService.Received(1).Hash("123456");
    }

    [Test]
    public void Create_ShouldStoreHashedCode()
    {
        var otp = OtpCode.Create("123456", _hashService,_now);

        otp.Hash.Should().Be("hashed-code");
    }

    [Test]
    public void Create_ShouldSetExpiresAt_WithDefaultTtl()
    {
        var before = _now.AddMinutes(10);
        var otp = OtpCode.Create("123456", _hashService,_now);
        var after = _now.AddMinutes(10);

        otp.ExpiresAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Test]
    public void Create_ShouldSetExpiresAt_WithCustomTtl()
    {
        var before = _now.AddMinutes(5);
        var otp = OtpCode.Create("123456", _hashService, _now,minutesTtl: 5);
        var after = _now.AddMinutes(5);

        otp.ExpiresAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Test]
    public void Create_ShouldSetUsedAt_AsNull()
    {
        var otp = OtpCode.Create("123456", _hashService,_now);

        otp.UsedAt.Should().BeNull();
    }

    // -------------------------
    // IsExpired
    // -------------------------

    [Test]
    public void IsExpired_ShouldReturnFalse_WhenCodeIsNotExpired()
    {
        var otp = OtpCode.Create("123456", _hashService,_now, minutesTtl: 10);

        otp.IsExpired(_now).Should().BeFalse();
    }

    [Test]
    public void IsExpired_ShouldReturnTrue_WhenCodeIsExpired()
    {
        var otp = BuildExpiredOtpCode();

        otp.IsExpired(_now).Should().BeTrue();
    }

    // -------------------------
    // IsAlreadyUsed
    // -------------------------

    [Test]
    public void IsAlreadyUsed_ShouldReturnFalse_WhenCodeHasNotBeenUsed()
    {
        var otp = OtpCode.Create("123456", _hashService,_now);

        otp.IsAlreadyUsed().Should().BeFalse();
    }

    [Test]
    public void IsAlreadyUsed_ShouldReturnTrue_WhenCodeHasBeenUsed()
    {
        var otp = BuildUsedOtpCode();

        otp.IsAlreadyUsed().Should().BeTrue();
    }

    // -------------------------
    // Validate
    // -------------------------

    [Test]
    public void Validate_ShouldReturnTrue_WhenCodeIsValid()
    {
        var otp = BuildValidOtpCode();

        var result = otp.Validate("123456", _hashService,_now);

        result.Should().BeTrue();
    }

    [Test]
    public void Validate_ShouldThrowOtpCodeExpiredException_WhenCodeIsExpired()
    {
        var otp = BuildExpiredOtpCode();

        var act = () => otp.Validate("123456", _hashService,_now);

        act.Should().Throw<OtpCodeExpiredException>();
    }

    [Test]
    public void Validate_ShouldThrowOtpCodeAlreadyUsedException_WhenCodeHasBeenUsed()
    {
        var otp = BuildUsedOtpCode();
        _hashService.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var act = () => otp.Validate("123456", _hashService,_now);

        act.Should().Throw<OtpCodeAlreadyUsedException>();
    }

    [Test]
    public void Validate_ShouldThrowInvalidOtpCodeException_WhenCodeIsWrong()
    {
        var otp = OtpCode.Create("123456", _hashService,_now, minutesTtl: 10);
        _hashService.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => otp.Validate("999999", _hashService,_now);

        act.Should().Throw<InvalidOtpCodeException>();
    }

    [Test]
    public void Validate_ShouldCheckExpiration_BeforeAlreadyUsed()
    {
        var otp = BuildExpiredOtpCode();
        otp.MarkAsUsed();

        var act = () => otp.Validate("123456", _hashService,_now);

        act.Should().Throw<OtpCodeExpiredException>();
    }

    [Test]
    public void Validate_ShouldCheckAlreadyUsed_BeforeWrongCode()
    {
        var otp = BuildUsedOtpCode();
        _hashService.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => otp.Validate("999999", _hashService,_now);

        act.Should().Throw<OtpCodeAlreadyUsedException>();
    }

    // -------------------------
    // MarkAsUsed
    // -------------------------

    [Test]
    public void MarkAsUsed_ShouldSetUsedAt()
    {
        var otp = OtpCode.Create("123456", _hashService,_now);
        var before = DateTime.UtcNow;

        otp.MarkAsUsed();

        otp.UsedAt.Should().NotBeNull();
        otp.UsedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Test]
    public void MarkAsUsed_ShouldMakeIsAlreadyUsed_ReturnTrue()
    {
        var otp = OtpCode.Create("123456", _hashService,_now);

        otp.MarkAsUsed();

        otp.IsAlreadyUsed().Should().BeTrue();
    }
}