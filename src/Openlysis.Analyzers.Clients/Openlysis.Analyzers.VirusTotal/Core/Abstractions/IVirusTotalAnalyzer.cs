using ErrorOr;

using Openlysis.Analyzers.VirusTotal.Core.Models.Responses;

namespace Openlysis.Analyzers.VirusTotal.Core.Abstractions;

/// <summary>
/// Interface for analyzing URLs using VirusTotal.
/// </summary>
public interface IVirusTotalAnalyzer
{
    /// <summary>
    /// Analyzes a URL asynchronously.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="requestFactory">The factory to create the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis response.</returns>
    public Task<ErrorOr<AnalyzeUrlResponse>> AnalyzeAsync(
        HttpClient httpClient,
        IRequestFactory requestFactory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the analysis result asynchronously.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <param name="id">The ID of the analysis to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis result.</returns>
    public Task<ErrorOr<GetAnalysisResponse>> GetAnalysisAsync(
        HttpClient httpClient,
        string id,
        CancellationToken cancellationToken = default);
}