
using System.Security.Cryptography;
using FootballCatch.Common.BuildingBlocks;


namespace FootballCatch.Auth.Domain.Users.Aggregates
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
        /// <summary>
        /// foreing key from a device
        /// </summary>
        public Guid DeviceTokenId { get; private set; }
        /// <summary>
        /// chain of characters that represents a refresh token
        /// </summary>
        public string Token { get; private set; }
        /// <summary>
        /// Expiring time of a token
        /// </summary>
        public DateTime ExpiresAt { get; private set; }
        /// <summary>
        /// Date in which token was revoked
        /// </summary>
        public DateTime? RevokedAt { get; private set; }
        /// <summary>
        /// Previous token from the current token
        /// </summary>
        public string? ReplacedTokenBy { get; private set; }
        /// <summary>
        /// Ip which created the token
        /// </summary>
        public string CreatedByIp{get;private set;}
        /// <summary>
        /// Creates a refresh token
        /// </summary>
        /// <param name="DeviceUid"></param>
        /// <param name="ip"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static RefreshToken Create(Guid DeviceUid, string ip, DateTime now)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return new RefreshToken
            (
                Guid.CreateVersion7(),
                DeviceUid,
                token,
                now.AddDays(30),
                null,
                null,
                ip
            );
        }
    }

}
