
using System.Security.Cryptography;
using FootballCatch.Common.BuildingBlocks;


namespace FootballCatch.Auth.Domain.users.Aggregates
{
    public sealed class RefreshToken : Entity<Guid>
    {
        private RefreshToken
        (
            Guid id, 
            Guid deviceTokenId, 
            string token, 
            DateTime expiresAt, 
            DateTime? revokedAt, 
            string? replacedTokenBy,
            string createdByIp
        ): base(id)
        {
            DeviceTokenId = deviceTokenId;
            Token = token;
            ExpiresAt = expiresAt;
            RevokedAt = revokedAt;
            ReplacedTokenBy = replacedTokenBy;
            CreatedByIp=createdByIp;
        }

        public Guid DeviceTokenId { get; private set; }
        public string Token { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? ReplacedTokenBy { get; private set; }
        public string CreatedByIp{get;private set;}

        public static RefreshToken Create(Guid DeviceUid, string ip, DateTime now)
        {
             var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return new RefreshToken(Guid.CreateVersion7(),DeviceUid,token,now.AddDays(30),null, null, ip);
        }
    }

}
