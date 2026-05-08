namespace FootballCatch.Common.Contracts.Catalog;

/// <summary>
/// Published by the Catalog service when a new competition is created.
/// Consumed by Fixtures to keep its local competition projection up to date.
/// </summary>
public sealed record CompetitionCreatedV1(
    Guid CompetitionId,
    string Name,
    string CountryIsoCode,
    string CountryDisplayName,
    int SeasonStartYear,
    int SeasonEndYear,
    string? LogoUrl,
    string? ExternalId,
    DateTime OccurredAtUtc
) : IIntegrationEvent
{
    /// <inheritdoc />
    public string EventName => EventNames.Catalog.CompetitionCreated;
}
