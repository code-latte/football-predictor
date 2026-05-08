using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Events;

/// <summary>Raised when an existing competition's details are modified or when it is reactivated.</summary>
public sealed record CompetitionUpdatedDomainEvent(
    CompetitionId CompetitionId,
    DateTime OccurredAtUtc
) : IDomainEvent;
