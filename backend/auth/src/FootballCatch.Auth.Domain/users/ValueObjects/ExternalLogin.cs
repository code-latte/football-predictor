using System;
using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Types;

namespace FootballCatch.Auth.Domain.Users.ValueObjects;

public record ExternalLogin : ValueObject
{
    private ExternalLogin(string provider, string providerUserId, string? providerEmail, DateTime linkedAt)
    {
        Provider = provider;
        ProviderUserId = providerUserId;
        ProviderEmail = providerEmail;
        LinkedAt = linkedAt;
    }

    public string Provider { get; init; }      
    public string ProviderUserId { get; init; } 
    public string? ProviderEmail { get; init; }
    public DateTime LinkedAt { get; init; }

    public static ExternalLogin Create
    (
        string provider,
        string providerUserId,
        DateTime now,
        string? providerEmail=null
        
    )
    {
        return new ExternalLogin
        (
            provider,
            providerUserId,
            providerEmail,
            now
        );    
    }
}
