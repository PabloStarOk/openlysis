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

    // For EF core.
#pragma warning disable CS8618
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class
    /// to be used by EF Core.
    /// </summary>
    protected Entity()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    /// Gets ID of the entity.
    /// </summary>
    public TId Id { get; }

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
