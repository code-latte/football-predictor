
using FootballCatch.Common.BuildingBlocks;

namespace FootballCatch.Auth.Domain.Users.Aggregates
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
        /// <summary>
        /// reference to user
        /// </summary>
        public Guid UserId {get;private set;}
        /// <summary>
        /// Identifier of the device (it will be created by the front-end)
        /// </summary>
        public string DeviceIdentifier {get; private set;}
        /// <summary>
        /// Name of the device
        /// </summary>
        public string DeviceName{get; private set;}
        /// <summary>
        /// Device platform (IOS, Android)
        /// </summary>
        public string Platform{get; private set;}
        /// <summary>
        /// Date in which user was logged with this device
        /// </summary>
        public DateTime LastSeenAt{get; private set;}
        /// <summary>
        /// Created Date
        /// </summary>
        public DateTime CreatedAt{get; private set;}
        /// <summary>
        /// Date in which device was revoked
        /// </summary>
        public DateTime? RevokedAt{get; private set;}
        private readonly List<RefreshToken> _refreshTokens = new();
        /// <summary>
        /// List of refresh tokens
        /// </summary>
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        /// <summary>
        /// determines if token is active
        /// </summary>
        public bool IsActive => RevokedAt is null;
        /// <summary>
        /// emits a token
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public RefreshToken IssueRefreshToken(string ip, DateTime now)
        {
            var token = RefreshToken.Create(this.Id, ip,now);
            _refreshTokens.Add(token);
            return token;
        }
        /// <summary>
        /// updates the date of the last log in of a device
        /// </summary>
        /// <param name="now"></param>
        public void UpdateLastSeen(DateTime now)
        {
            LastSeenAt=now;
        }

        
    }
}
