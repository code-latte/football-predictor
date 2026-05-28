namespace FootballCatch.Auth.Domain.Exceptions;

/// <summary>
/// Base class for all domain exceptions raised within the Auth bounded context.
/// Catching this type provides a single catch-all for any Auth domain violation.
/// </summary>
public abstract class AuthDomainExceptions : Exception
{
    protected AuthDomainExceptions(string message) : base(message)
    {
        
    }
}