using ErrorOr;

namespace Openlysis.Evaluators.Shared.Abstractions;

/// <summary>
/// Interface for parsing HTTP responses into a specific model.
/// </summary>
/// <typeparam name="TModel">The type of the model to parse.</typeparam>
public interface IResponseParser<TModel>
    where TModel : notnull
{
    /// <summary>
    /// Parses the HTTP response into a specific model asynchronously.
    /// </summary>
    /// <param name="response">The HTTP response message to parse.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ErrorOr object with the parsed model.</returns>
    public Task<ErrorOr<TModel>> ParseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default);
}