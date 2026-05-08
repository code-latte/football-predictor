using FootballCatch.Catalog.Domain.Aggregates;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Repositories;

/// <summary>
/// Repository contract for the <see cref="Competition"/> aggregate.
/// Concrete implementations live in the Infrastructure layer.
/// </summary>
public interface ICompetitionRepository
{
    /// <summary>Returns the competition with the given ID, or <c>null</c> if not found.</summary>
    Task<Competition?> GetByIdAsync(CompetitionId id, CancellationToken ct = default);

    /// <summary>Returns the competition with the given external provider ID, or <c>null</c> if not found.</summary>
    Task<Competition?> GetByExternalIdAsync(string externalId, CancellationToken ct = default);

    /// <summary>Persists a new competition to the store.</summary>
    Task AddAsync(Competition competition, CancellationToken ct = default);

    /// <summary>Persists changes made to an existing competition.</summary>
    Task UpdateAsync(Competition competition, CancellationToken ct = default);

    /// <summary>Returns <c>true</c> when a competition with the given ID exists in the store.</summary>
    Task<bool> ExistsAsync(CompetitionId id, CancellationToken ct = default);
}
