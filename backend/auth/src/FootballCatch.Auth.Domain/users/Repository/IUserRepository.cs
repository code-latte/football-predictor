using System;
using FootballCatch.Auth.Domain.Users.Aggregates;

namespace FootballCatch.Auth.Domain.Users.Repository;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<User?> FindByExternalLoginAsync(string provider,string providerUserId, CancellationToken cancellationToken);
    Task<User?> FindByEmailAsync(string Email, CancellationToken cancellationToken);
    Task AddOtpCodeAsync(string HashCode);
    Task<OtpCode> GetHashedCodeAsync(string Email);
    

}
