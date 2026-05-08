using FootballCatch.Common.BuildingBlocks;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Events;

/// <summary>Raised when a team is created or updated (upserted) in the Catalog.</summary>
public sealed record TeamUpsertedDomainEvent(
    TeamId TeamId,
    DateTime OccurredAtUtc
) : IDomainEvent;
