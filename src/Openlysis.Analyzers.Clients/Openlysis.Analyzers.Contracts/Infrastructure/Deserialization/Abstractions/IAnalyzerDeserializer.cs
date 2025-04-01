using System.Text.Json;

using ErrorOr;

namespace Openlysis.Analyzers.Contracts.Infrastructure.Deserialization.Abstractions;

/// <summary>
/// Defines a deserializer for analyzers.
/// </summary>
public interface IAnalyzerDeserializer
{
    /// <summary>
    /// Deserializes a JSON element into an instance of the specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to deserialize to.</typeparam>
    /// <param name="rootElement">The JSON element to deserialize.</param>
    /// <returns>An <see cref="ErrorOr{TModel}"/> containing the deserialized model or an error.</returns>
    public ErrorOr<TModel> Deserialize<TModel>(JsonElement rootElement)
        where TModel : class;

    /// <summary>
    /// Deserializes a JSON element into an instance of the specified model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message containing the JSON element to deserialize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="ErrorOr{TModel}"/> containing the deserialized model or an error.</returns>
    public Task<ErrorOr<TModel>> DeserializeAsync<TModel>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TModel : class;
}