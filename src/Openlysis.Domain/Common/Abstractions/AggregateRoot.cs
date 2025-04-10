namespace Openlysis.Domain.Common.Abstractions;

/// <summary>
/// Represents an aggregate root which encapsulates entities
/// and value objects.
/// </summary>
/// <typeparam name="TId">Unique identifier of the aggregate root.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">ID of the aggregate root.</param>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

#pragma warning disable CS8618
#pragma warning disable S1144
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class
    /// to be used by EF Core.
    /// </summary>
    protected AggregateRoot()
    {
    }
#pragma warning restore S1144
#pragma warning restore CS8618
}
