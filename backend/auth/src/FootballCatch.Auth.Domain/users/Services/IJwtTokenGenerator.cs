using System;
using FootballCatch.Auth.Domain.Users.Aggregates;

namespace FootballCatch.Auth.Domain.Users.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);   
}
