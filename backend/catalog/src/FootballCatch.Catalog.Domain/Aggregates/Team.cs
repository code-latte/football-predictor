using FootballCatch.Catalog.Domain.Events;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;
using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Aggregates;

/// <summary>
/// Aggregate root for a football team.
/// Both creation and updates emit <see cref="TeamUpsertedDomainEvent"/> so downstream services
/// (e.g. Fixtures) always receive a consistent projection-refresh event.
/// </summary>
public sealed class Team : AggregateRoot<TeamId>
{
    /// <summary>Full name of the team.</summary>
    public TeamName Name { get; private set; }

    /// <summary>Short abbreviation, 2–5 uppercase letters (e.g. "MCI", "MUFC").</summary>
    public TeamShortCode ShortCode { get; private set; }

    /// <summary>Country the team is based in.</summary>
    public Country Country { get; private set; }

    /// <summary>Optional URL to the team's crest image.</summary>
    public string? CrestUrl { get; private set; }

    /// <summary>
    /// Immutable identifier from the upstream football data provider.
    /// Once set it cannot be changed — see <see cref="ExternalIdImmutableException"/>.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>UTC timestamp of when this team record was first created.</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>UTC timestamp of the last mutation to this team.</summary>
    public DateTime UpdatedAtUtc { get; private set; }

    private Team(
        TeamId id,
        TeamName name,
        TeamShortCode shortCode,
        Country country,
        string? crestUrl,
        string? externalId,
        DateTime utcNow)
        : base(id)
    {
        Name = name;
        ShortCode = shortCode;
        Country = country;
        CrestUrl = crestUrl;
        ExternalId = externalId;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    /// <summary>
    /// Creates a new team and raises <see cref="TeamUpsertedDomainEvent"/>.
    /// </summary>
    public static Team Create(
        TeamName name,
        TeamShortCode shortCode,
        Country country,
        string? crestUrl,
        string? externalId,
        DateTime utcNow)
    {
        Team team = new(
            TeamId.New(),
            name,
            shortCode,
            country,
            crestUrl,
            externalId,
            utcNow);

        team.RaiseDomainEvent(new TeamUpsertedDomainEvent(team.Id, utcNow));

        return team;
    }

    /// <summary>
    /// Updates the team's mutable properties and raises <see cref="TeamUpsertedDomainEvent"/>.
    /// </summary>
    /// <param name="name">New full name.</param>
    /// <param name="shortCode">New short code.</param>
    /// <param name="country">New country.</param>
    /// <param name="crestUrl">New crest URL (may be null to clear).</param>
    /// <param name="utcNow">Timestamp of the operation.</param>
    /// <exception cref="ExternalIdImmutableException">
    /// Thrown internally if a caller (not shown here) tries to re-assign <see cref="ExternalId"/>.
    /// The method signature does not accept a new external ID because the ID is set at creation only.
    /// </exception>
    public void Update(
        TeamName name,
        TeamShortCode shortCode,
        Country country,
        string? crestUrl,
        DateTime utcNow)
    {
        Name = name;
        ShortCode = shortCode;
        Country = country;
        CrestUrl = crestUrl;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new TeamUpsertedDomainEvent(Id, utcNow));
    }

    /// <summary>
    /// Attempts to set the external provider ID.
    /// </summary>
    /// <exception cref="ExternalIdImmutableException">
    /// Thrown when the external ID has already been set to a different value.
    /// Setting the same value again is silently accepted (idempotent).
    /// </exception>
    public void SetExternalId(string externalId, DateTime utcNow)
    {
        if (ExternalId is not null && ExternalId != externalId)
        {
            throw new ExternalIdImmutableException(nameof(Team), ExternalId);
        }

        if (ExternalId == externalId)
        {
            return;
        }

        ExternalId = externalId;
        UpdatedAtUtc = utcNow;
    }

    public static Team Rehydrate(
        TeamId id,
        TeamName name,
        TeamShortCode shortCode,
        Country country,
        string? crestUrl,
        string? externalId,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Team team = new(id, name, shortCode, country, crestUrl, externalId, createdAtUtc);
        team.UpdatedAtUtc = updatedAtUtc;
        return team;
    }
}
