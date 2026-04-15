namespace FootballCatch.Common.BuildingBlocks;

/// <summary>
/// Base class for all domain entities with a strongly-typed identifier.
/// Equality is identity-based: two entities with the same Id are the same entity,
/// regardless of the state of their other properties.
/// </summary>
/// <typeparam name="TId">The strongly-typed ID type (e.g. <c>readonly record struct PredictionId(Guid Value)</c>).</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        Entity<TId> other = (Entity<TId>)obj;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
