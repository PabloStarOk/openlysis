namespace Openlysis.Analyzers.VirusTotal.Core.Models.Objects;

/// <summary>
/// Represents a request to analyze content with VirusTotal service.
/// </summary>
/// <param name="Address">Address of the endpoint to submit the content to analyze.</param>
/// <param name="Content">The content of the request.</param>
public sealed record VirusTotalAnalysisRequest(
    string Address,
    HttpContent Content)
    : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
        Content.Dispose();
    }
}