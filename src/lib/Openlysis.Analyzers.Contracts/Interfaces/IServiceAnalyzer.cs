using ErrorOr;

using Openlysis.Analyzers.Contracts.Requests;

namespace Openlysis.Analyzers.Contracts.Interfaces;

/// <summary>
/// Interface for analyzing services.
/// </summary>
/// <typeparam name="TAnalysis">The type of the analysis result.</typeparam>
/// <typeparam name="TAnalysisId">The type of the analysis identifier.</typeparam>
public interface IServiceAnalyzer<TAnalysis, TAnalysisId>
    where TAnalysis : notnull
    where TAnalysisId : notnull
{
    /// <summary>
    /// Gets the name of the service which provides the analyzer.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Analyzes a file.
    /// </summary>
    /// <param name="request">A <see cref="FileAnalysisRequest"/> containing the file data and other parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <typeparamref name="TAnalysisId"/> representing the result of the analysis.</returns>
    public Task<ErrorOr<TAnalysisId>> AnalyzeAsync(FileAnalysisRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the analysis result for a given analysis ID.
    /// </summary>
    /// <param name="id">The ID of the analysis to retrieve.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <typeparamref name="TAnalysis"/> representing the analysis result.</returns>
    public Task<ErrorOr<TAnalysis>> GetAnalysisAsync(TAnalysisId id, CancellationToken cancellationToken = default);
}