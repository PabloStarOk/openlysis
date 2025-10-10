using System.Linq.Expressions;

using Openlysis.Domain.Common.Abstractions;
using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.Common.Abstractions.Persistence;

/// <summary>
/// Defines a generic repository for data access operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IRepository<TEntity>
    where TEntity : Entity<GlobalId>
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public Task<TEntity?> GetAsync(GlobalId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single entity matching the specified filter expression, with optional ordering.
    /// </summary>
    /// <param name="filter">An expression to filter entities.</param>
    /// <param name="orderBy">An optional function to order the filtered entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple entities based on a matching expression, with paging and optional ordering.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of entities to retrieve per page.</param>
    /// <param name="filter">An optional expression to filter entities.</param>
    /// <param name="orderBy">An optional function to order the retrieved entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the retrieved entities.</returns>
    public Task<IReadOnlyList<TEntity>> GetManyAsync(
        int page,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple entities by their identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the entities if found.</returns>
    public Task<IReadOnlyList<TEntity>> GetManyByIdsAsync(
        GlobalId[] ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(TEntity entity);

    /// <summary>
    /// Persists all changes made in the repository to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous save operation.</returns>
    public Task SaveChangeAsync(CancellationToken cancellationToken = default);
}