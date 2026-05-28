namespace FootballCatch.Auth.Domain.Exceptions;

/// <summary>
/// Thrown when the life time of the verification code has been reached
/// </summary>
public sealed class OtpCodeExpiredException
: AuthDomainExceptions
{
    public OtpCodeExpiredException()
    : base("Your verification code has expired")
    {
        
    }
}
/// <summary>
/// Thrown when the code had been used
/// </summary>
public sealed class OtpCodeAlreadyUsedException
: AuthDomainExceptions
{
    public OtpCodeAlreadyUsedException()
    : base("The verification code had been already used")
    {
        
    }
}
/// <summary>
/// Thrown when the code that user introduced does not match with the one that is stored in database
/// </summary>
public sealed class InvalidOtpCodeException
: AuthDomainExceptions
{
    public InvalidOtpCodeException()
    : base("The verification code is wrong")
    {
        
    }
}