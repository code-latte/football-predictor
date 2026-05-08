using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Events;

/// <summary>Raised when a new competition is created in the Catalog.</summary>
public sealed record CompetitionCreatedDomainEvent(
    CompetitionId CompetitionId,
    DateTime OccurredAtUtc
) : IDomainEvent;
