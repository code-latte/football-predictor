namespace FootballCatch.Common.Contracts.Catalog;

/// <summary>
/// Published by the Catalog service when a team is created or updated (upserted).
/// Consumed by Fixtures to keep its local team projection up to date.
/// </summary>
public sealed record TeamUpsertedV1(
    Guid TeamId,
    string Name,
    string ShortCode,
    string CountryIsoCode,
    string CountryDisplayName,
    string? CrestUrl,
    string? ExternalId,
    DateTime OccurredAtUtc
) : IIntegrationEvent
{
    /// <inheritdoc />
    public string EventName => EventNames.Catalog.TeamUpserted;
}
