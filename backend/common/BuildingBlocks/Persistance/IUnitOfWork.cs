namespace FootballCatch.Common.BuildingBlocks.Persistance;
/// <summary>
/// Defines a contract for committing changes made within a unit of work to the underlying persistence store
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the underlying store as a single atomic transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}