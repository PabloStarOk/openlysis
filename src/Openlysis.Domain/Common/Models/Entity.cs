namespace Openlysis.Domain.Common.Models;

/// <summary>
/// Represents an entity.
/// </summary>
/// <typeparam name="TId">Unique identifier of the entity.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    /// <param name="id">ID of the entity.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets or sets ID of the entity.
    /// </summary>
    public TId Id { get; protected init; }

    public static bool operator ==(Entity<TId> left, Entity<TId> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId> left, Entity<TId> right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    /// <inheritdoc/>
    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
