using System.Text.Json;

namespace Openlysis.Analyzers.Contracts.Infrastructure.Logging.Abstractions;

/// <summary>
/// Defines a basic and custom logger for analyzers.
/// </summary>
public interface IAnalyzerLogger
{
    /// <summary>
    /// Logs non-success HTTP responses including status code and response content.
    /// </summary>
    /// <param name="response">The HTTP response message containing status and content.</param>
    /// <param name="cancellationToken">Token to propagate notification that operations should be canceled.</param>
    /// <returns>A task that represents the asynchronous logging operation.</returns>
    public Task LogNonSuccessStatusCodeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs failures that occur during JSON deserialization with contextual data.
    /// </summary>
    /// <param name="targetType">The type attempting to deserialize into.</param>
    /// <param name="exception">The specific exception thrown during deserialization.</param>
    /// <param name="problematicJson">The JSON fragment that caused deserialization failure.</param>
    public void LogDeserializationFailure(
        Type targetType,
        Exception exception,
        JsonElement problematicJson);

    /// <summary>
    /// Logs situations where deserialization completes but returns null for non-nullable types.
    /// </summary>
    /// <param name="targetType">The expected type that received null.</param>
    public void LogUnexpectedNullResult(Type targetType);
}