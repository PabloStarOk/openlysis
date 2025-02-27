using System.Linq.Expressions;

namespace Openlysis.Application.Common.Interfaces.Persistence;

/// <summary>
/// Defines a generic repository for data access operations.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TModelId">The type of the model identifier.</typeparam>
public interface IRepository<TModel, in TModelId>
    where TModel : notnull
    where TModelId : notnull
{
    /// <summary>
    /// Retrieves a model by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the model.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The model if found; otherwise, null.</returns>
    public Task<TModel?> GetAsync(TModelId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple models based on a matching expression.
    /// </summary>
    /// <param name="amount">The number of models to retrieve. Default is 10.</param>
    /// <param name="filter">The expression to match models.</param>
    /// <param name="orderBy">An optional function to order the retrieved models.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the models if found; otherwise, null.</returns>
    public Task<IEnumerable<TModel>> GetManyAsync(
        int amount = 10,
        Expression<Func<TModel, bool>>? filter = null,
        Func<IQueryable<TModel>, IOrderedQueryable<TModel>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new model to the repository.
    /// </summary>
    /// <param name="model">The model to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddAsync(TModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing model in the repository.
    /// </summary>
    /// <param name="analysis">The model to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task UpdateAsync(TModel analysis, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a model with the specified identifier exists in the repository.
    /// </summary>
    /// <param name="id">The identifier of the model.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a result of <c>true</c> if the model exists; otherwise, <c>false</c>.</returns>
    public Task<bool> ExistsAsync(TModelId id, CancellationToken cancellationToken = default);
}