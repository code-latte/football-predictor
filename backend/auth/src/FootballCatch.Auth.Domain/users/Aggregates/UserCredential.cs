
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Auth.Domain.Users.Aggregates;

public sealed class UserCredential : Entity<Guid>
{
    private UserCredential
    (
        Guid id, 
        string passwordHash, 
        DateTime lastChangedAt
    ) : base(id)
    {
        PasswordHash = passwordHash;
        LastChangedAt = lastChangedAt;
    }
    /// <summary>
    /// Hashed password from a user
    /// </summary>
    public string PasswordHash{get; private set;}
    /// <summary>
    /// Date in which user changed the password
    /// </summary>
    public DateTime LastChangedAt{get;private set;}
    /// <summary>
    /// Creates a user credential
    /// </summary>
    /// <param name="passwordHash"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public static UserCredential Create(string passwordHash, DateTime now)
    {
        return new UserCredential
        (
            Guid.CreateVersion7(),
            passwordHash,
            now
        );
    }
}
