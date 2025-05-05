using ErrorOr;

namespace Openlysis.Application.Common.Abstractions.Contracts;

/// <summary>
/// Interface for evaluating the reputation of data of type <typeparamref name="TData"/>
/// and returning a model of type <typeparamref name="TModel"/> with the results.
/// </summary>
/// <typeparam name="TData">The type of the request data to evaluate.</typeparam>
/// <typeparam name="TModel">The type of the model to return after evaluation.</typeparam>
public interface IReputationEvaluator<in TData, TModel>
    where TData : notnull
    where TModel : notnull
{
    /// <summary>
    /// Gets the name of the reputation evaluation service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets a value indicating whether the reputation evaluation service is currently available.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Evaluates the reputation of the given data and returns a model of type <typeparamref name="TModel"/>.
    /// </summary>
    /// <param name="data">The data to evaluate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an <see cref="ErrorOr{T}"/> object with the evaluated model.
    /// </returns>
    public Task<ErrorOr<TModel>> EvaluateAsync(
        TData data,
        CancellationToken cancellationToken = default);
}