namespace Openlysis.Domain.Common.Models;

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
}
