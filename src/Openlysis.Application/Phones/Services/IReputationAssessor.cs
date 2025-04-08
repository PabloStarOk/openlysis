using ErrorOr;

using Openlysis.Application.Common.Requests;

namespace Openlysis.Application.Phones.Services;

/// <summary>
/// Interface for assessing data of type <typeparamref name="TData"/> and returning a model of type <typeparamref name="TModel"/>.
/// </summary>
/// <typeparam name="TData">The type of the data to assess.</typeparam>
/// <typeparam name="TModel">The type of the model to return after assessment.</typeparam>
public interface IReputationAssessor<in TData, TModel>
    where TData : AssessData
    where TModel : notnull
{
    /// <summary>
    /// Gets the name of the assessor service.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets a value indicating whether the assessor service is available.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Assesses the given data and returns a model of type <typeparamref name="TModel"/>.
    /// </summary>
    /// <param name="data">The data to assess.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ErrorOr object with the assessed model.</returns>
    public Task<ErrorOr<TModel>> AssessAsync(
        TData data,
        CancellationToken cancellationToken = default);
}