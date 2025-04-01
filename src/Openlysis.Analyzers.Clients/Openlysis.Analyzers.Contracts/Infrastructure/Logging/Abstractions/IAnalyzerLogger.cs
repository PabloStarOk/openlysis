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
}