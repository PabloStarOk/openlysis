using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Application.Common.Interfaces.Ports;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;
using Openlysis.FileMultiAnalysisWorker.Configuration;

namespace Openlysis.FileMultiAnalysisWorker.Consumer;

/// <summary>
/// Consumer class for handling file analysis job requests.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IConsumer{T}"/> interface to process <see cref="FileAnalysisJobRequest"/> messages.
/// </remarks>
public class FileMultiAnalysisConsumer : IConsumer<FileAnalysisJobRequest>
{
    private readonly ILogger<FileMultiAnalysisConsumer> _logger;
    private readonly IOptionsMonitor<AnalysisConsumerSettings> _options;
    private readonly IEnumerable<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>> _analyzers;
    private ConsumeContext<FileAnalysisJobRequest> _context;
    private FileMultiAnalysis _multiAnalysis;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMultiAnalysisConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log messages.</param>
    /// <param name="options">The options monitor for file analysis consumer options.</param>
    /// <param name="analyzers">The collection of service analyzers to use for file analysis.</param>
    public FileMultiAnalysisConsumer(
        ILogger<FileMultiAnalysisConsumer> logger,
        IOptionsMonitor<AnalysisConsumerSettings> options,
        IEnumerable<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>> analyzers)
    {
        _logger = logger;
        _options = options;
        _analyzers = analyzers;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<FileAnalysisJobRequest> context)
    {
        _context = context;
        _multiAnalysis = context.Message.FileMultiAnalysis;

        await AnalyzeAsync(context.CancellationToken);
        await UpdateAnalysisAsync(context.CancellationToken);
        await PublishUpdateAsync();
        await context.Message.FileStreamData.DisposeAsync();
    }

    /// <summary>
    /// Analyzes the file using the available analyzers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task AnalyzeAsync(CancellationToken cancellationToken)
    {
        FileAnalysisJobRequest request = _context.Message;
        ConcurrentBag<ServiceFileAnalysis> analyses = [];

        await Parallel.ForEachAsync(_analyzers, cancellationToken, async (analyzer, ct) =>
        {
            var analyzeResult = await analyzer.AnalyzeAsync(request, ct);
            if (analyzeResult.IsError)
            {
                _logger.LogError("One or more errors occurred while analyzing a file: {Errors}", analyzeResult.Errors);
                return;
            }

            var analysisResult = await analyzer.GetAnalysisAsync(analyzeResult.Value, ct);
            if (analysisResult.IsError)
            {
                _logger.LogError("One or more errors occurred while getting analysis results: {Errors}", analysisResult.Errors);
                return;
            }

            analyses.Add(analysisResult.Value);
        });

        while (analyses.TryTake(out ServiceFileAnalysis analysis))
        {
            _multiAnalysis.AddServiceAnalysis(analysis);
        }

        await PublishUpdateAsync();
    }

    /// <summary>
    /// Updates the analysis asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateAnalysisAsync(CancellationToken cancellationToken)
    {
        ConcurrentBag<ServiceFileAnalysis> analyses = [];
        var analyzersMap = _analyzers.ToDictionary(a => a.ServiceName);
        AnalysisStatus lastStatus = AnalysisStatus.Queued;

        while (!cancellationToken.IsCancellationRequested &&
               (_multiAnalysis.Status is not AnalysisStatus.Finished and not AnalysisStatus.Timeout))
        {
            await Parallel.ForEachAsync(
                _multiAnalysis.ServiceFileAnalyses, cancellationToken, async (analysis, token) =>
                {
                    var analyzer = analyzersMap[analysis.ServiceName];
                    var result = await analyzer.GetAnalysisAsync(analysis.Id, token);

                    if (result.IsError)
                    {
                        _logger.LogError("One or more errors occurred while updating file service analysis {Error}", result.Errors);
                        return;
                    }

                    analyses.Add(result.Value);
                });

            while (analyses.TryTake(out ServiceFileAnalysis analysis))
            {
                _multiAnalysis.UpdateServiceAnalysis(analysis);
            }

            // Only update if status has changed.
            if (_multiAnalysis.Status != lastStatus)
            {
                await PublishUpdateAsync();
                lastStatus = _multiAnalysis.Status;
            }

            await Task.Delay(_options.CurrentValue.UpdateFrequencyMs, cancellationToken);
        }
    }

    /// <summary>
    /// Publishes the updated file analysis to the message bus.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task PublishUpdateAsync()
    {
        await _context.Publish(_multiAnalysis, _context.CancellationToken);
    }
}