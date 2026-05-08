using FluentAssertions;
using FootballCatch.Catalog.Domain.Aggregates;
using FootballCatch.Catalog.Domain.Events;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.UnitTests.Aggregates;

[TestFixture]
public sealed class TeamTests
{
    private static readonly DateTime UtcNow = new(2024, 5, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Team CreateValidTeam(string? externalId = "ext-team-001") =>
        Team.Create(
            new TeamName("Manchester City"),
            new TeamShortCode("MCI"),
            new Country("GB", "United Kingdom"),
            crestUrl: "https://crest.url",
            externalId: externalId,
            utcNow: UtcNow);

    // ─── Create ────────────────────────────────────────────────────────────────

    [Test]
    public void Create_WithValidArguments_ShouldReturnTeamWithCorrectProperties()
    {
        // Act
        Team team = CreateValidTeam();

        // Assert
        team.Name.Value.Should().Be("Manchester City");
        team.ShortCode.Value.Should().Be("MCI");
        team.Country.IsoCode.Should().Be("GB");
        team.CrestUrl.Should().Be("https://crest.url");
        team.ExternalId.Should().Be("ext-team-001");
        team.CreatedAtUtc.Should().Be(UtcNow);
        team.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Test]
    public void Create_WithValidArguments_ShouldRaiseTeamUpsertedDomainEvent()
    {
        // Act
        Team team = CreateValidTeam();

        // Assert
        team.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TeamUpsertedDomainEvent>()
            .Which.TeamId.Should().Be(team.Id);
    }

    [Test]
    public void Create_ShouldSetOccurredAtUtcOnDomainEvent()
    {
        // Act
        Team team = CreateValidTeam();

        // Assert
        team.DomainEvents.OfType<TeamUpsertedDomainEvent>()
            .Single().OccurredAtUtc.Should().Be(UtcNow);
    }

    [Test]
    public void Create_WithNullExternalId_ShouldSucceed()
    {
        // Act
        Team team = CreateValidTeam(externalId: null);

        // Assert
        team.ExternalId.Should().BeNull();
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    [Test]
    public void Update_WithValidArguments_ShouldUpdatePropertiesAndRaiseTeamUpsertedDomainEvent()
    {
        // Arrange
        Team team = CreateValidTeam();
        team.ClearDomainEvents();
        DateTime updateTime = UtcNow.AddHours(1);

        // Act
        team.Update(
            new TeamName("Man City FC"),
            new TeamShortCode("MCFC"),
            new Country("GB", "United Kingdom"),
            crestUrl: "https://new-crest.url",
            utcNow: updateTime);

        // Assert
        team.Name.Value.Should().Be("Man City FC");
        team.ShortCode.Value.Should().Be("MCFC");
        team.CrestUrl.Should().Be("https://new-crest.url");
        team.UpdatedAtUtc.Should().Be(updateTime);

        team.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TeamUpsertedDomainEvent>()
            .Which.TeamId.Should().Be(team.Id);
    }

    [Test]
    public void Update_ToNullCrestUrl_ShouldClearCrestUrl()
    {
        // Arrange
        Team team = CreateValidTeam();
        team.ClearDomainEvents();

        // Act
        team.Update(
            team.Name,
            team.ShortCode,
            team.Country,
            crestUrl: null,
            utcNow: UtcNow.AddHours(1));

        // Assert
        team.CrestUrl.Should().BeNull();
    }

    // ─── SetExternalId ─────────────────────────────────────────────────────────

    [Test]
    public void SetExternalId_WhenExternalIdIsNull_ShouldSetValue()
    {
        // Arrange
        Team team = CreateValidTeam(externalId: null);

        // Act
        team.SetExternalId("new-ext-id", UtcNow.AddMinutes(1));

        // Assert
        team.ExternalId.Should().Be("new-ext-id");
    }

    [Test]
    public void SetExternalId_WithSameValueAsExisting_ShouldBeIdempotent()
    {
        // Arrange
        Team team = CreateValidTeam(externalId: "ext-001");
        string originalUpdatedAt = team.UpdatedAtUtc.ToString("O");

        // Act — setting to the same value should not update UpdatedAtUtc
        team.SetExternalId("ext-001", UtcNow.AddMinutes(1));

        // Assert
        team.ExternalId.Should().Be("ext-001");
        team.UpdatedAtUtc.ToString("O").Should().Be(originalUpdatedAt);
    }

    [Test]
    public void SetExternalId_WithDifferentValueWhenAlreadySet_ShouldThrowExternalIdImmutableException()
    {
        // Arrange
        Team team = CreateValidTeam(externalId: "ext-001");

        // Act
        Action act = () => team.SetExternalId("ext-002", UtcNow.AddMinutes(1));

        // Assert
        act.Should().Throw<ExternalIdImmutableException>();
    }

    // ─── Rehydrate ─────────────────────────────────────────────────────────────

    [Test]
    public void Rehydrate_WithAllProperties_ShouldRestoreStateExactly()
    {
        // Arrange
        TeamId id = TeamId.New();
        TeamName name = new("Manchester City");
        TeamShortCode shortCode = new("MCI");
        Country country = new("GB", "United Kingdom");
        string crestUrl = "https://crest.url";
        string externalId = "ext-team-001";
        DateTime createdAtUtc = UtcNow;
        DateTime updatedAtUtc = UtcNow.AddDays(1);

        // Act
        Team team = Team.Rehydrate(id, name, shortCode, country, crestUrl, externalId, createdAtUtc, updatedAtUtc);

        // Assert
        team.Id.Should().Be(id);
        team.Name.Value.Should().Be("Manchester City");
        team.ShortCode.Value.Should().Be("MCI");
        team.Country.IsoCode.Should().Be("GB");
        team.CrestUrl.Should().Be(crestUrl);
        team.ExternalId.Should().Be(externalId);
        team.CreatedAtUtc.Should().Be(createdAtUtc);
        team.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Test]
    public void Rehydrate_WithNullExternalId_ShouldRestoreNullExternalId()
    {
        // Act
        Team team = Team.Rehydrate(
            TeamId.New(),
            new TeamName("Manchester City"),
            new TeamShortCode("MCI"),
            new Country("GB", "United Kingdom"),
            crestUrl: "https://crest.url",
            externalId: null,
            createdAtUtc: UtcNow,
            updatedAtUtc: UtcNow.AddDays(1));

        // Assert
        team.ExternalId.Should().BeNull();
    }

    [Test]
    public void Rehydrate_ShouldNotRaiseAnyDomainEvents()
    {
        // Act
        Team team = Team.Rehydrate(
            TeamId.New(),
            new TeamName("Manchester City"),
            new TeamShortCode("MCI"),
            new Country("GB", "United Kingdom"),
            crestUrl: "https://crest.url",
            externalId: "ext-team-001",
            createdAtUtc: UtcNow,
            updatedAtUtc: UtcNow.AddDays(1));

        // Assert
        team.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Rehydrate_WhenCreatedAndUpdatedTimestampsDiffer_ShouldPreserveBoth()
    {
        // Arrange
        DateTime createdAtUtc = UtcNow;
        DateTime updatedAtUtc = UtcNow.AddDays(1);

        // Act
        Team team = Team.Rehydrate(
            TeamId.New(),
            new TeamName("Manchester City"),
            new TeamShortCode("MCI"),
            new Country("GB", "United Kingdom"),
            crestUrl: "https://crest.url",
            externalId: "ext-team-001",
            createdAtUtc: createdAtUtc,
            updatedAtUtc: updatedAtUtc);

        // Assert
        team.CreatedAtUtc.Should().Be(createdAtUtc);
        team.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Test]
    public void Rehydrate_ShouldProduceAggregateWithSuppliedId()
    {
        // Arrange
        TeamId id = TeamId.New();

        // Act
        Team team = Team.Rehydrate(
            id,
            new TeamName("Manchester City"),
            new TeamShortCode("MCI"),
            new Country("GB", "United Kingdom"),
            crestUrl: "https://crest.url",
            externalId: "ext-team-001",
            createdAtUtc: UtcNow,
            updatedAtUtc: UtcNow.AddDays(1));

        // Assert
        team.Id.Should().Be(id);
    }
}
