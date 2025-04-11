using Openlysis.Domain.Common.ValueObjects;

namespace Openlysis.Application.Common.Abstractions.Services;

/// <summary>
/// Defines a service that uses various analysis services to analyze a URL.
/// </summary>
public interface IUrlMultiAnalyzer
{
    /// <summary>
    /// Starts the analysis of the given URL for the specified user.
    /// </summary>
    /// <param name="multiAnalysisId">The ID of the multi-analysis.</param>
    /// <param name="url">The URL to be analyzed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task StartAnalysisAsync(
        GlobalId multiAnalysisId,
        Uri url,
        CancellationToken cancellationToken = default);
}