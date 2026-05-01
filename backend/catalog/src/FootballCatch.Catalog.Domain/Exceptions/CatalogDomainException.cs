namespace FootballCatch.Catalog.Domain.Exceptions;

/// <summary>
/// Base class for all domain exceptions raised within the Catalog bounded context.
/// Catching this type provides a single catch-all for any Catalog domain violation.
/// </summary>
public abstract class CatalogDomainException : Exception
{
    protected CatalogDomainException(string message) : base(message)
    {
    }
}
