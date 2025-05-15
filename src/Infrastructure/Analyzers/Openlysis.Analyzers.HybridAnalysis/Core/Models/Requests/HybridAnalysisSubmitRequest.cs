namespace Openlysis.Analyzers.HybridAnalysis.Core.Models.Requests;

/// <summary>
/// Represents a request to submit a sample for analysis to the Hybrid Analysis service.
/// </summary>
/// <param name="EndpointAddress">The API endpoint address to send the request to.</param>
/// <param name="HttpContent">The HTTP content containing the sample data to be analyzed.</param>
internal sealed record HybridAnalysisSubmitRequest(
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