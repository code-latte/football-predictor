using FootballCatch.Auth.Domain.Exceptions;
using FootballCatch.Auth.Domain.Users.Services;
using FootballCatch.Common.BuildingBlocks;

public record OtpCode : ValueObject
{
    public string Hash { get; }
    public DateTime ExpiresAt { get; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt{get;}

    private OtpCode(string hash, DateTime expiresAt,DateTime createdAt)
    {
        Hash = hash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public static OtpCode Create(string plainCode, IHashService hashService,DateTime now, int minutesTtl = 10)
    {
        var hash = hashService.Hash(plainCode);
        return new OtpCode(hash, now.AddMinutes(minutesTtl),now);
    }

    public bool IsExpired(DateTime now) => now > ExpiresAt;

    public bool IsAlreadyUsed() => UsedAt.HasValue;

    public bool Validate(string plainCode, IHashService hashService,DateTime now)
        {
        if (IsExpired(now)) throw new OtpCodeExpiredException();
        if (IsAlreadyUsed()) throw new OtpCodeAlreadyUsedException();
        if (!hashService.Verify(plainCode, Hash)) throw new InvalidOtpCodeException();

        return true;
    }

    public void MarkAsUsed() => UsedAt = DateTime.UtcNow;
}