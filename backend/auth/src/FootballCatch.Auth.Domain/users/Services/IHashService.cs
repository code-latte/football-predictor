namespace FootballCatch.Auth.Domain.Users.Services;

public interface IHashService
{
    string Hash(string value);
    bool Verify(string plain,string hash);
}