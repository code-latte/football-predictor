using FootballCatch.Common.Mediator;

namespace FootballCatch.Auth.Application.Users.Commands.OtpLogin.VerifyOtpCode;

public sealed record VerifyOtpCodeCommand
(
    string Code,
    string Ip,
    string DeviceIdentifier,
    string DeviceName,
    string Platform,
    string Email
) : ICommand<OtpLoginResult>;

public record OtpLoginResult(string AccessToken,string RefreshToken,bool IsNewUser);