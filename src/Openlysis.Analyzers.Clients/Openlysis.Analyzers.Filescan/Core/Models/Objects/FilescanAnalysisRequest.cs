namespace Openlysis.Analyzers.Filescan.Core.Models.Objects;

/// <summary>
/// Represents a request to be sent to Filescan.io service to analyze content.
/// </summary>
/// <param name="EndpointAddress">Address of the endpoint to send the request.</param>
/// <param name="HttpContent">Content of the HTTP request.</param>
public sealed record FilescanAnalysisRequest(
    string EndpointAddress,
    HttpContent HttpContent)
    : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
        HttpContent.Dispose();
    }
}