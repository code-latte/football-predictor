using System;

namespace FootballCatch.Auth.Domain.users.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);   
}
