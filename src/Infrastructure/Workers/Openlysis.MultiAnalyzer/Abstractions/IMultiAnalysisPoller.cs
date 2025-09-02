using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Openlysis.Domain.Common.Entities;

namespace Openlysis.MultiAnalyzer.Abstractions;

/// <summary>
/// Defines a poller that monitors and retrieves updated analyses of type <typeparamref name="TAnalysis"/>.
/// </summary>
/// <typeparam name="TAnalysis">The type of analysis to poll, must inherit from <see cref="Analysis"/>.</typeparam>
internal interface IMultiAnalysisPoller<TAnalysis>
    where TAnalysis : Analysis
{
    /// <summary>
    /// Gets a channel reader that provides arrays of updated analyses.
    /// </summary>s
    public ChannelReader<TAnalysis[]> UpdatedAnalyses { get; }

    /// <summary>
    /// Polls the specified analyses for updates asynchronously.
    /// </summary>
    /// <param name="pollableAnalyses">The analyses to poll for updates.</param>
    /// <param name="cancellationToken">A token to cancel the polling operation.</param>
    /// <returns>A task representing the asynchronous polling operation.</returns>
    public Task PollAsync(TAnalysis[] pollableAnalyses, CancellationToken cancellationToken = default);
}