using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ErrorOr;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.Files.Requests;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.Files.Entities;
using Openlysis.Infrastructure.Shared.Communication.Abstractions;
using Openlysis.Infrastructure.Shared.Communication.Models;
using Openlysis.MultiAnalyzer.Configuration;

namespace Openlysis.MultiAnalyzer.Communication.Consumers.Files;

/// <summary>
/// Consumer class for handling file analysis job requests.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IConsumer{T}"/> interface to process <see cref="AnalyzeFile"/> messages.
/// </remarks>
public class AnalyzeFileConsumer : IConsumer<AnalyzeFile>
{
    private readonly ILogger<AnalyzeFileConsumer> _logger;
    private readonly IOptionsMonitor<AnalyzeConsumerOptions> _options;
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly IDictionary<string, Analyzer<FileAnalysis, AnalyzeFileRequest>> _analyzers;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly Dictionary<ComposedAnalysisId, FileAnalysis> _pendingAnalyses = [];
    private readonly ConcurrentBag<FileAnalysis> _updatableAnalyses = [];

    private GlobalId _multiAnalysisId;
    private ConsumeContext<AnalyzeFile> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log messages.</param>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="options">The options monitor for file analysis consumer options.</param>
    /// <param name="fileStorageProvider">The provider for file storage operations.</param>
    /// <param name="analyzers">The collection of service analyzers to use for file analysis.</param>
    public AnalyzeFileConsumer(
        ILogger<AnalyzeFileConsumer> logger,
        IEndpointUriProvider endpointUriProvider,
        IOptionsMonitor<AnalyzeConsumerOptions> options,
        IFileStorageProvider fileStorageProvider,
        IEnumerable<Analyzer<FileAnalysis, AnalyzeFileRequest>> analyzers)
    {
        _logger = logger;
        _endpointUriProvider = endpointUriProvider;
        _options = options;
        _fileStorageProvider = fileStorageProvider;
        _analyzers = analyzers.ToDictionary(a => a.ServiceName);
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<AnalyzeFile> context)
    {
        _context = context;
        _multiAnalysisId = context.Message.FileMultiAnalysisId;

        await AnalyzeAsync(context.CancellationToken);
        await UpdateAnalysisStatusAsync(context.CancellationToken);
    }

    /// <summary>
    /// Analyzes the file using the available analyzers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task AnalyzeAsync(CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            _analyzers.Values,
            cancellationToken,
            async (analyzer, ct) =>
        {
            await using var fileStream = await DownloadFileAsync(ct);

            var request = new AnalyzeFileRequest(
                fileStream,
                DownloadFileAsync,
                _context.Message.FileName,
                _context.Message.FileContentType,
                _context.Message.FilePassword,
                _context.Message.FileSha256,
                _context.Message.IsPrivateFile);

            var analyzeResult = await analyzer.AnalyzeAsync(request, ct);
            if (analyzeResult.IsError)
            {
                _logger.LogError("One or more errors occurred while analyzing a file: {Errors}", analyzeResult.Errors);
                return;
            }

            FileAnalysis analysis = analyzeResult.Value;
            _pendingAnalyses.Add(analysis.Id, analysis);
        });

        await Task.WhenAll(
            _fileStorageProvider.DeleteAsync(_context.Message.FileId, cancellationToken),
            SendUpdateAsync(_pendingAnalyses.Values.ToArray()));
    }

    /// <summary>
    /// Updates the analysis asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateAnalysisStatusAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested
               && _pendingAnalyses.Count > 0)
        {
            await RunRequestsBatchAsync(cancellationToken);
            await Task.Delay(_options.CurrentValue.RequestBatchWaitTimeMs, cancellationToken);
        }
    }

    /// <summary>
    /// Executes a batch of requests asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RunRequestsBatchAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < _options.CurrentValue.RequestsPerBatch; i++)
        {
            await RunBatchCycleAsync(cancellationToken);

            if (!_updatableAnalyses.IsEmpty)
            {
                await SendUpdateAsync(_updatableAnalyses.ToArray());
                _updatableAnalyses.Clear();
            }

            if (_pendingAnalyses.Count is 0)
            {
                break;
            }

            await Task.Delay(_options.CurrentValue.RequestFrequencyMs, cancellationToken);
        }
    }

    /// <summary>
    /// Executes an HTTP request for each <see cref="FileAnalysis"/> to update them.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RunBatchCycleAsync(CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            _pendingAnalyses.Values,
            cancellationToken,
            async (analysis, ct) =>
            {
                var analyzer = _analyzers[analysis.ServiceName];
                if (!analyzer.CanGetAnalysisStatus)
                {
                    return;
                }

                // Get status
                ErrorOr<AnalysisStatus> getStatusResult
                    = await analyzer.GetStatusAsync(analysis.Id, ct);
                if (getStatusResult.IsError)
                {
                    _logger.LogError(
                        "One or more errors occurred while updating file service analysis:"
                        + "\n\t{Error}",
                        getStatusResult.Errors);
                    analysis.UpdateStatus(AnalysisStatus.Failed);
                    _pendingAnalyses.Remove(analysis.Id);
                    _updatableAnalyses.Add(analysis);
                    return;
                }

                // Update status
                if (getStatusResult.Value
                    is AnalysisStatus.Queued
                    or AnalysisStatus.InProgress)
                {
                    return;
                }

                if (!analyzer.CanGetAnalysis)
                {
                    _logger.LogError("Could not get analysis because services is unavailable.");
                    return;
                }

                // Get full analysis
                ErrorOr<FileAnalysis> getAnalysisResult
                    = await analyzer.GetAnalysisAsync(analysis.Id, ct);
                if (getAnalysisResult.IsError)
                {
                    _logger.LogError(
                        "One error or more occurred while getting service analysis:"
                        + "\n\t{Error}",
                        getAnalysisResult.Errors);
                    return;
                }

                analysis = getAnalysisResult.Value;
                _pendingAnalyses.Remove(analysis.Id);
                _updatableAnalyses.Add(analysis);
            });
    }

    /// <summary>
    /// Downloads the file from the file storage provider.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation with a stream containing the downloaded file data.</returns>
    private async Task<Stream> DownloadFileAsync(
        CancellationToken cancellationToken = default)
    {
        return await _fileStorageProvider.DownloadAsync(
            _context.Message.FileId,
            cancellationToken);
    }

    /// <summary>
    /// Sends an update of file service analyses to the appropriate endpoint.
    /// </summary>
    /// <param name="analyses">The file service analyses to update. If none are provided, all pending analyses will be sent.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SendUpdateAsync(params FileAnalysis[] analyses)
    {
        var request = new UpdateFileMultiAnalysis(
            _multiAnalysisId,
            analyses);
        await _context.Send(_endpointUriProvider.UpdateFileMultiAnalysisUri, request);
    }
}