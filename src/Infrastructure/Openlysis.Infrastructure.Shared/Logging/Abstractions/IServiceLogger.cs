using Microsoft.Extensions.Logging;

namespace Openlysis.Infrastructure.Shared.Logging.Abstractions;

/// <summary>
/// Defines a basic and custom logger for analyzers.
/// </summary>
/// <summary>
/// Represents an abstract base class for a service logger that implements <see cref="ILogger{TCategoryName}"/>.
/// </summary>
/// <typeparam name="TCategoryName">The category name type for the logger, which must be non-nullable.</typeparam>
public interface IServiceLogger<out TCategoryName> : ILogger<TCategoryName>
    where TCategoryName : notnull
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