namespace FootballCatch.Common.BuildingBlocks;

/// <summary>
/// Base record for all value objects.
/// Value objects have no identity — two instances are equal when all their components are equal.
/// Subclasses declare their components as positional parameters or <c>init</c> properties;
/// the compiler generates correct structural equality automatically.
/// </summary>
/// <example>
/// <code>
/// public sealed record Score(int Home, int Away) : ValueObject;
/// public sealed record InviteCode(string Value) : ValueObject;
/// </code>
/// </example>
public abstract record ValueObject;
