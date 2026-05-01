
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Auth.Domain.users.Aggregates;

public sealed class UserCredential : Entity<Guid>
{
    private UserCredential(Guid id, string passwordHash, DateTime lastChangedAt) : base(id)
    {
        PasswordHash = passwordHash;
        LastChangedAt = lastChangedAt;
    }

    public string PasswordHash{get; private set;}
    public DateTime LastChangedAt{get;private set;}

    public static UserCredential Crear(string passwordHash, DateTime now)
    {
        return new UserCredential(Guid.CreateVersion7(),passwordHash,now);
    }
}
