using Openlysis.Application.Common.Interfaces.Ports;
using Openlysis.Domain.FileAnalyses;

namespace Openlysis.Application.Common.Interfaces.Services;

/// <summary>
/// Defines a service to analyze a file.
/// </summary>
public interface IFileMultiAnalysisService
{
    /// <summary>
    /// Starts a multi analysis job asynchronously.
    /// </summary>
    /// <param name="request">The request containing the analysis details.</param>
    /// <param name="multiAnalysis">The multiAnalysis object to be updated.</param>
    /// <param name="cancellationToken">The cancellation token to cancel asynchronous the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task StartJobAsync(
        FileAnalysisJobRequest request,
        FileMultiAnalysis multiAnalysis,
        CancellationToken cancellationToken);
}