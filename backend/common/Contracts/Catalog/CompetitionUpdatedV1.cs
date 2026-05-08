namespace FootballCatch.Common.Contracts.Catalog;

/// <summary>
/// Published by the Catalog service when an existing competition is updated or reactivated.
/// </summary>
public sealed record CompetitionUpdatedV1(
    Guid CompetitionId,
    string Name,
    string CountryIsoCode,
    string CountryDisplayName,
    int SeasonStartYear,
    int SeasonEndYear,
    string? LogoUrl,
    bool IsActive,
    DateTime OccurredAtUtc
) : IIntegrationEvent
{
    /// <inheritdoc />
    public string EventName => EventNames.Catalog.CompetitionUpdated;
}
