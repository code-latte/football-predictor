using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Events;

/// <summary>Raised when a competition is deactivated in the Catalog.</summary>
public sealed record CompetitionDeactivatedDomainEvent(
    CompetitionId CompetitionId,
    DateTime OccurredAtUtc
) : IDomainEvent;
