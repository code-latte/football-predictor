namespace FootballCatch.Auth.Domain.Users.Services;

public interface IEmailSender
{
    Task Send(string recipient, string subject, string body);
}