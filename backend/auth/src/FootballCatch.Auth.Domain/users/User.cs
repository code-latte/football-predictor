
using FootballCatch.Auth.Domain.Exceptions;
using FootballCatch.Auth.Domain.users.Aggregates;
using FootballCatch.Auth.Domain.users.ValueObjects;
using FootballCatch.Common.BuildingBlocks;


namespace FootballCatch.Auth.Domain.users
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
        public string Email {get;private set;}
        public string FullName {get;private set;}
        private List<DeviceToken> _deviceTokens = new();
        public IReadOnlyCollection<DeviceToken> DeviceTokens => _deviceTokens.AsReadOnly();
        private List<ExternalLogin> _externalLogins=new();

        public IReadOnlyCollection<ExternalLogin> ExternalLogins => _externalLogins.AsReadOnly();

        public static User CreateWithExternalProvider
        (
            string email,
            string fullName,
            string provider,
            string providerUserId,
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
                    providerEmail
                )
            );
            return user;
        }
        public void JoinUserWithExternalProvider
        (
            string provider,
            string providerUserId,
            string? providerEmail=null
        )
        {
            if (IsProviderAlreadyLinkedToUser(provider,providerUserId))
            {
                throw new DomainException($"Provider {provider} is already linked to this account");
            }

            this._externalLogins.Add
            (
                ExternalLogin.Create
                (
                    provider,
                    providerUserId,
                    providerEmail
                )
            );

        }
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
        public bool IsProviderAlreadyLinkedToUser(string provider, string providerUserId)
        {
            ExternalLogin? externallogin=_externalLogins.FirstOrDefault(p=>p.Provider==provider && p.ProviderUserId==providerUserId);
            return externallogin is not null;
        }
    }
}
