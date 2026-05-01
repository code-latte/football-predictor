
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Auth.Domain.users.Aggregates
{
    public sealed class DeviceToken : Entity<Guid>
    {
        internal DeviceToken
        (
            Guid id,
            Guid userId, 
            string deviceIdentifier,
            string deviceName, 
            string platform, 
            DateTime lastSeenAt, 
            DateTime createdAt, 
            DateTime? revokedAt
        ) : base(id)
        {
            UserId = userId;
            DeviceIdentifier = deviceIdentifier;
            DeviceName = deviceName;
            Platform = platform;
            LastSeenAt = lastSeenAt;
            CreatedAt = createdAt;
            RevokedAt = revokedAt;
        }

        public Guid UserId {get;private set;}
        public string DeviceIdentifier {get; private set;}
        public string DeviceName{get; private set;}
        public string Platform{get; private set;}
        public DateTime LastSeenAt{get; private set;}
        public DateTime CreatedAt{get; private set;}
        public DateTime? RevokedAt{get; private set;}
        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        public bool IsActive => RevokedAt is null;
        // Este metodo se llama asi por que en ingles Issue es algo parecido a emitir
        public RefreshToken IssueRefreshToken(string ip, DateTime now)
        {
            var token = RefreshToken.Create(this.Id, ip,now);
            _refreshTokens.Add(token);
            return token;
        }
        public void UpdateLastSeen(DateTime now)
        {
            LastSeenAt=now;
        }

        
    }
}
