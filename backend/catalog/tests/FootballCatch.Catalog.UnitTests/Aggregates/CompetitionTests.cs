using FluentAssertions;
using FootballCatch.Catalog.Domain.Aggregates;
using FootballCatch.Catalog.Domain.Events;
using FootballCatch.Catalog.Domain.Exceptions;
using FootballCatch.Catalog.Domain.ValueObjects;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.UnitTests.Aggregates;

[TestFixture]
public sealed class CompetitionTests
{
    private static readonly DateTime UtcNow = new(2024, 5, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Competition CreateValidCompetition(string? externalId = "ext-001") =>
        Competition.Create(
            new CompetitionName("Premier League"),
            new Country("GB", "United Kingdom"),
            new Season(2023, 2024),
            logoUrl: null,
            externalId: externalId,
            utcNow: UtcNow);

    // ─── Create ────────────────────────────────────────────────────────────────

    [Test]
    public void Create_WithValidArguments_ShouldReturnActiveCompetition()
    {
        // Act
        Competition competition = CreateValidCompetition();

        // Assert
        competition.Name.Value.Should().Be("Premier League");
        competition.Country.IsoCode.Should().Be("GB");
        competition.Season.StartYear.Should().Be(2023);
        competition.Season.EndYear.Should().Be(2024);
        competition.IsActive.Should().BeTrue();
        competition.ExternalId.Should().Be("ext-001");
        competition.CreatedAtUtc.Should().Be(UtcNow);
        competition.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Test]
    public void Create_WithValidArguments_ShouldRaiseCompetitionCreatedDomainEvent()
    {
        // Act
        Competition competition = CreateValidCompetition();

        // Assert
        competition.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompetitionCreatedDomainEvent>()
            .Which.CompetitionId.Should().Be(competition.Id);
    }

    [Test]
    public void Create_ShouldSetOccurredAtUtcOnDomainEvent()
    {
        // Act
        Competition competition = CreateValidCompetition();

        // Assert
        competition.DomainEvents.OfType<CompetitionCreatedDomainEvent>()
            .Single().OccurredAtUtc.Should().Be(UtcNow);
    }

    // ─── Update ────────────────────────────────────────────────────────────────

    [Test]
    public void Update_WhenActive_ShouldUpdatePropertiesAndRaiseCompetitionUpdatedDomainEvent()
    {
        // Arrange
        Competition competition = CreateValidCompetition();
        competition.ClearDomainEvents();
        DateTime updateTime = UtcNow.AddHours(1);

        // Act
        competition.Update(
            new CompetitionName("La Liga"),
            new Country("ES", "Spain"),
            new Season(2024, 2024),
            logoUrl: "https://logo.url",
            utcNow: updateTime);

        // Assert
        competition.Name.Value.Should().Be("La Liga");
        competition.Country.IsoCode.Should().Be("ES");
        competition.Season.Label.Should().Be("2024");
        competition.LogoUrl.Should().Be("https://logo.url");
        competition.UpdatedAtUtc.Should().Be(updateTime);

        competition.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompetitionUpdatedDomainEvent>()
            .Which.CompetitionId.Should().Be(competition.Id);
    }

    [Test]
    public void Update_WhenInactive_ShouldThrowCompetitionInactiveException()
    {
        // Arrange
        Competition competition = CreateValidCompetition();
        competition.Deactivate(UtcNow);

        // Act
        Action act = () => competition.Update(
            new CompetitionName("La Liga"),
            new Country("ES", "Spain"),
            new Season(2024, 2024),
            logoUrl: null,
            utcNow: UtcNow.AddHours(1));

        // Assert
        act.Should().Throw<CompetitionInactiveException>();
    }

    // ─── Deactivate ────────────────────────────────────────────────────────────

    [Test]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalseAndRaiseDeactivatedEvent()
    {
        // Arrange
        Competition competition = CreateValidCompetition();
        competition.ClearDomainEvents();
        DateTime deactivateTime = UtcNow.AddHours(2);

        // Act
        competition.Deactivate(deactivateTime);

        // Assert
        competition.IsActive.Should().BeFalse();
        competition.UpdatedAtUtc.Should().Be(deactivateTime);

        competition.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompetitionDeactivatedDomainEvent>()
            .Which.CompetitionId.Should().Be(competition.Id);
    }

    [Test]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowCompetitionAlreadyDeactivatedException()
    {
        // Arrange
        Competition competition = CreateValidCompetition();
        competition.Deactivate(UtcNow);

        // Act
        Action act = () => competition.Deactivate(UtcNow.AddMinutes(5));

        // Assert
        act.Should().Throw<CompetitionAlreadyDeactivatedException>();
    }

    // ─── Reactivate ────────────────────────────────────────────────────────────

    [Test]
    public void Reactivate_WhenInactive_ShouldSetIsActiveToTrueAndRaiseCompetitionUpdatedDomainEvent()
    {
        // Arrange
        Competition competition = CreateValidCompetition();
        competition.Deactivate(UtcNow);
        competition.ClearDomainEvents();
        DateTime reactivateTime = UtcNow.AddDays(1);

        // Act
        competition.Reactivate(reactivateTime);

        // Assert
        competition.IsActive.Should().BeTrue();
        competition.UpdatedAtUtc.Should().Be(reactivateTime);

        competition.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompetitionUpdatedDomainEvent>();
    }

    [Test]
    public void Reactivate_WhenAlreadyActive_ShouldBeIdempotentAndNotRaiseEvents()
    {
        // Arrange
        Competition competition = CreateValidCompetition();
        competition.ClearDomainEvents();

        // Act
        competition.Reactivate(UtcNow.AddHours(1));

        // Assert
        competition.IsActive.Should().BeTrue();
        competition.DomainEvents.Should().BeEmpty();
    }

    // ─── ClearDomainEvents ─────────────────────────────────────────────────────

    [Test]
    public void ClearDomainEvents_ShouldRemoveAllCollectedEvents()
    {
        // Arrange
        Competition competition = CreateValidCompetition();
        competition.DomainEvents.Should().NotBeEmpty();

        // Act
        competition.ClearDomainEvents();

        // Assert
        competition.DomainEvents.Should().BeEmpty();
    }

    // ─── Rehydrate ─────────────────────────────────────────────────────────────

    [Test]
    public void Rehydrate_WithAllProperties_ShouldRestoreStateExactly()
    {
        // Arrange
        CompetitionId id = CompetitionId.New();
        CompetitionName name = new("Premier League");
        Country country = new("GB", "United Kingdom");
        Season season = new(2023, 2024);
        string logoUrl = "https://logo.url";
        string externalId = "ext-001";
        DateTime createdAtUtc = UtcNow;
        DateTime updatedAtUtc = UtcNow.AddDays(1);

        // Act
        Competition competition = Competition.Rehydrate(id, name, country, season, logoUrl, externalId, isActive: true, createdAtUtc, updatedAtUtc);

        // Assert
        competition.Id.Should().Be(id);
        competition.Name.Value.Should().Be("Premier League");
        competition.Country.IsoCode.Should().Be("GB");
        competition.Season.StartYear.Should().Be(2023);
        competition.Season.EndYear.Should().Be(2024);
        competition.LogoUrl.Should().Be(logoUrl);
        competition.ExternalId.Should().Be(externalId);
        competition.IsActive.Should().BeTrue();
        competition.CreatedAtUtc.Should().Be(createdAtUtc);
        competition.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Test]
    public void Rehydrate_WhenIsActiveFalse_ShouldRestoreInactiveState()
    {
        // Arrange & Act
        Competition competition = Competition.Rehydrate(
            CompetitionId.New(),
            new CompetitionName("Premier League"),
            new Country("GB", "United Kingdom"),
            new Season(2023, 2024),
            logoUrl: null,
            externalId: "ext-001",
            isActive: false,
            createdAtUtc: UtcNow,
            updatedAtUtc: UtcNow.AddDays(1));

        // Assert
        competition.IsActive.Should().BeFalse();
    }

    [Test]
    public void Rehydrate_ShouldNotRaiseAnyDomainEvents()
    {
        // Act
        Competition competition = Competition.Rehydrate(
            CompetitionId.New(),
            new CompetitionName("Premier League"),
            new Country("GB", "United Kingdom"),
            new Season(2023, 2024),
            logoUrl: null,
            externalId: "ext-001",
            isActive: true,
            createdAtUtc: UtcNow,
            updatedAtUtc: UtcNow.AddDays(1));

        // Assert
        competition.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Rehydrate_WhenCreatedAndUpdatedTimestampsDiffer_ShouldPreserveBoth()
    {
        // Arrange
        DateTime createdAtUtc = UtcNow;
        DateTime updatedAtUtc = UtcNow.AddDays(1);

        // Act
        Competition competition = Competition.Rehydrate(
            CompetitionId.New(),
            new CompetitionName("Premier League"),
            new Country("GB", "United Kingdom"),
            new Season(2023, 2024),
            logoUrl: null,
            externalId: "ext-001",
            isActive: true,
            createdAtUtc: createdAtUtc,
            updatedAtUtc: updatedAtUtc);

        // Assert
        competition.CreatedAtUtc.Should().Be(createdAtUtc);
        competition.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Test]
    public void Rehydrate_ShouldProduceAggregateWithSuppliedId()
    {
        // Arrange
        CompetitionId id = CompetitionId.New();

        // Act
        Competition competition = Competition.Rehydrate(
            id,
            new CompetitionName("Premier League"),
            new Country("GB", "United Kingdom"),
            new Season(2023, 2024),
            logoUrl: null,
            externalId: "ext-001",
            isActive: true,
            createdAtUtc: UtcNow,
            updatedAtUtc: UtcNow.AddDays(1));

        // Assert
        competition.Id.Should().Be(id);
    }
}
