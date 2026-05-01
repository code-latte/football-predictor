using FootballCatch.Catalog.Domain.Events;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;
using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Aggregates;

/// <summary>
/// Aggregate root for a football competition (league, cup, or tournament).
/// A competition belongs to a country, runs over a season, and can be deactivated
/// when it is no longer being tracked by the platform.
/// </summary>
public sealed class Competition : AggregateRoot<CompetitionId>
{
    /// <summary>Human-readable name of the competition.</summary>
    public CompetitionName Name { get; private set; }

    /// <summary>Country in which the competition takes place.</summary>
    public Country Country { get; private set; }

    /// <summary>The calendar season this competition covers.</summary>
    public Season Season { get; private set; }

    /// <summary>Optional URL to the competition's logo image.</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Immutable identifier from the upstream football data provider.
    /// Null until set by the Updater; once set it cannot be changed.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>Whether the competition is currently active and accepting fixture updates.</summary>
    public bool IsActive { get; private set; }

    /// <summary>UTC timestamp of when this competition record was first created.</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>UTC timestamp of the last mutation to this competition.</summary>
    public DateTime UpdatedAtUtc { get; private set; }

    private Competition(
        CompetitionId id,
        CompetitionName name,
        Country country,
        Season season,
        string? logoUrl,
        string? externalId,
        DateTime utcNow)
        : base(id)
    {
        Name = name;
        Country = country;
        Season = season;
        LogoUrl = logoUrl;
        ExternalId = externalId;
        IsActive = true;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    /// <summary>
    /// Creates a new competition and raises <see cref="CompetitionCreatedDomainEvent"/>.
    /// </summary>
    public static Competition Create(
        CompetitionName name,
        Country country,
        Season season,
        string? logoUrl,
        string? externalId,
        DateTime utcNow)
    {
        Competition competition = new(
            CompetitionId.New(),
            name,
            country,
            season,
            logoUrl,
            externalId,
            utcNow);

        competition.RaiseDomainEvent(new CompetitionCreatedDomainEvent(competition.Id, utcNow));

        return competition;
    }

    /// <summary>
    /// Updates the competition's mutable properties and raises <see cref="CompetitionUpdatedDomainEvent"/>.
    /// </summary>
    /// <exception cref="CompetitionInactiveException">Thrown when the competition is inactive.</exception>
    public void Update(
        CompetitionName name,
        Country country,
        Season season,
        string? logoUrl,
        DateTime utcNow)
    {
        if (!IsActive)
        {
            throw new CompetitionInactiveException(Id.Value);
        }

        Name = name;
        Country = country;
        Season = season;
        LogoUrl = logoUrl;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new CompetitionUpdatedDomainEvent(Id, utcNow));
    }

    /// <summary>
    /// Deactivates this competition and raises <see cref="CompetitionDeactivatedDomainEvent"/>.
    /// </summary>
    /// <exception cref="CompetitionAlreadyDeactivatedException">Thrown when already inactive.</exception>
    public void Deactivate(DateTime utcNow)
    {
        if (!IsActive)
        {
            throw new CompetitionAlreadyDeactivatedException(Id.Value);
        }

        IsActive = false;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new CompetitionDeactivatedDomainEvent(Id, utcNow));
    }

    /// <summary>
    /// Reactivates a previously deactivated competition and raises <see cref="CompetitionUpdatedDomainEvent"/>.
    /// Reactivation is idempotent: calling it on an already-active competition has no effect.
    /// </summary>
    public void Reactivate(DateTime utcNow)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new CompetitionUpdatedDomainEvent(Id, utcNow));
    }

    public static Competition Rehydrate(
        CompetitionId id,
        CompetitionName name,
        Country country,
        Season season,
        string? logoUrl,
        string? externalId,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Competition competition = new(id, name, country, season, logoUrl, externalId, createdAtUtc);
        competition.IsActive = isActive;
        competition.UpdatedAtUtc = updatedAtUtc;
        return competition;
    }
}
