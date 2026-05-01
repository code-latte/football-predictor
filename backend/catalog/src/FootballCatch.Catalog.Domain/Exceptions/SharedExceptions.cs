namespace FootballCatch.Catalog.Domain.Exceptions;

/// <summary>
/// Thrown when a caller attempts to change an external provider ID that has already been set.
/// External IDs are immutable once assigned — they are the stable link to the upstream data provider.
/// </summary>
public sealed class ExternalIdImmutableException : CatalogDomainException
{
    public ExternalIdImmutableException(string entityType, string existingExternalId)
        : base($"The external ID '{existingExternalId}' of {entityType} is immutable and cannot be changed.")
    {
    }
}
