using FootballCatch.Common.Mediator;

namespace FootballCatch.Auth.Application.Users.Commands.GoogleLogin;

public record class GoogleLoginCommand
(
    string Provider,        
    string ProviderUserId,  
    string Email,
    string FullName,
    string? ProviderEmail,
    string DeviceName,
    string Platform,
    string Ip,
    string DeviceIdentifier
) : ICommand<GoogleLoginResult>;
public record GoogleLoginResult(string AccessToken, string RefreshToken, bool IsNewUser);