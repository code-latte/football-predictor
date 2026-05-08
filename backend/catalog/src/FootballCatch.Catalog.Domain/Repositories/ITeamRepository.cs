using FootballCatch.Catalog.Domain.Aggregates;
using FootballCatch.Common.Types;

namespace FootballCatch.Catalog.Domain.Repositories;

/// <summary>
/// Repository contract for the <see cref="Team"/> aggregate.
/// Concrete implementations live in the Infrastructure layer.
/// </summary>
public interface ITeamRepository
{
    /// <summary>Returns the team with the given ID, or <c>null</c> if not found.</summary>
    Task<Team?> GetByIdAsync(TeamId id, CancellationToken ct = default);

    /// <summary>Returns the team with the given external provider ID, or <c>null</c> if not found.</summary>
    Task<Team?> GetByExternalIdAsync(string externalId, CancellationToken ct = default);

    /// <summary>Persists a new team to the store.</summary>
    Task AddAsync(Team team, CancellationToken ct = default);

    /// <summary>Persists changes made to an existing team.</summary>
    Task UpdateAsync(Team team, CancellationToken ct = default);

    /// <summary>Returns <c>true</c> when a team with the given ID exists in the store.</summary>
    Task<bool> ExistsAsync(TeamId id, CancellationToken ct = default);
}
