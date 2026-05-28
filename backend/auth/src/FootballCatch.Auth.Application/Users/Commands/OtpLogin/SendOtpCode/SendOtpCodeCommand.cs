using FootballCatch.Common.Mediator;

namespace FootballCatch.Auth.Application.Users.Commands.OtpLogin.SendOtpCode;

public sealed record SendOtpCodeCommand(string Email) : ICommand;