
using FootballCatch.Auth.Domain.Users.Aggregates;
using FootballCatch.Auth.Domain.Users.ValueObjects;
using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Exceptions;


namespace FootballCatch.Auth.Domain.Users.Aggregates
{
    public sealed class User : AggregateRoot<Guid>
    {
        private User
        (
            Guid id,
            string email,
            string fullName
            ) : base(id)
        {
            Email = email;
            FullName = fullName;
        }
        /// <summary>
        /// Email from User
        /// </summary>
        public string Email {get;private set;}
        /// <summary>
        /// Name and surnames from user
        /// </summary>
        public string FullName {get;private set;}
        private List<DeviceToken> _deviceTokens = new();
        /// <summary>
        /// List of devices linked to a user
        /// </summary>
        public IReadOnlyCollection<DeviceToken> DeviceTokens => _deviceTokens.AsReadOnly();
        private List<ExternalLogin> _externalLogins=new();
        /// <summary>
        /// List of providers linked to a user
        /// </summary>
        public IReadOnlyCollection<ExternalLogin> ExternalLogins => _externalLogins.AsReadOnly();
        /// <summary>
        ///  Create a user with a external provider (Google, github...)
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fullName"></param>
        /// <param name="provider"></param>
        /// <param name="providerUserId"></param>
        /// <param name="now"></param>
        /// <param name="providerEmail"></param>
        /// <returns></returns>
        /// <exception cref="DomainException"></exception>
        public static User CreateUserAndLinkToExternalProvider
        (
            string email,
            string fullName,
            string provider,
            string providerUserId,
            DateTime now,
            string? providerEmail=null
        )
        {
   
            User user=new User
            (
                Guid.CreateVersion7(),
                email,
                fullName
            );
            user._externalLogins.Add
            (
                ExternalLogin.Create
                (
                    provider,
                    providerUserId,
                    now,
                    providerEmail
                )
            );
            return user;
        }
        /// <summary>
        /// Link an existing user (registered by other method or linked to other provider) to an external provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="providerUserId"></param>
        /// <param name="now"></param>
        /// <param name="providerEmail"></param>
        /// <exception cref="DomainException"></exception>
        public void LinkUserWithExternalProvider
        (
            string provider,
            string providerUserId,
            DateTime now,
            string? providerEmail=null
        )
        {
            if (IsProviderAlreadyLinkedToUser(provider,providerUserId))
            {
                throw new DomainException($"Provider {provider} is already linked to an account");
            }

            this._externalLogins.Add
            (
                ExternalLogin.Create
                (
                    provider,
                    providerUserId,
                    now,
                    providerEmail
                )
            );

        }
        /// <summary>
        /// Register a device and links it to a user
        /// </summary>
        /// <param name="deviceIdentifier"></param>
        /// <param name="deviceName"></param>
        /// <param name="platform"></param>
        /// <param name="now"></param>
        /// <param name="revokedAt"></param>
        /// <returns></returns>
        public DeviceToken RegisterDevice
        (
            string deviceIdentifier,
            string deviceName,
            string platform,
            DateTime now,
            DateTime? revokedAt=null 
        )
        {
            DeviceToken? existing=_deviceTokens.FirstOrDefault(d=>d.DeviceIdentifier==deviceIdentifier);
            if (existing is not null)
            {
                existing.UpdateLastSeen(now);
                return existing;
            }
            else
            {
            DeviceToken device= new DeviceToken
            (
                Guid.CreateVersion7(),
                this.Id,
                deviceIdentifier,
                deviceName,
                platform,
                now,
                now,
                revokedAt
            );
            _deviceTokens.Add(device);
            return device;
            }
        }
        /// <summary>
        /// determines if a user is linked to a provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="providerUserId"></param>
        /// <returns></returns>
        public bool IsProviderAlreadyLinkedToUser
        (
            string provider, 
            string providerUserId
        )
        {
            ExternalLogin? externallogin=_externalLogins.FirstOrDefault(p=>p.Provider==provider && p.ProviderUserId==providerUserId);
            return externallogin is not null;
        }
    }
}
