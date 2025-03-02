using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.AnalysisWorker.Configuration;
using Openlysis.AnalysisWorker.Consumers.UpdateFileMultiAnalysis;
using Openlysis.Analyzers.Contracts.Interfaces;
using Openlysis.Analyzers.Contracts.Requests;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.AnalysisWorker.Consumers.AnalyzeFile;

/// <summary>
/// Consumer class for handling file analysis job requests.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IConsumer{T}"/> interface to process <see cref="AnalyzeFile"/> messages.
/// </remarks>
public class AnalyzeFileConsumer : IConsumer<AnalyzeFile>
{
    /// <summary>
    /// Name of the endpoint.
    /// </summary>
    public const string EndpointName = "analyze-file";

    private readonly ILogger<AnalyzeFileConsumer> _logger;
    private readonly IOptions<BrokerSettings> _brokerOptions;
    private readonly IOptionsMonitor<AnalyzeFileConsumerSettings> _options;
    private readonly Uri _updateAnalysisEndpointUri;
    private readonly IEnumerable<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>> _analyzers;
    private readonly Dictionary<ServiceFileAnalysisId, ServiceFileAnalysis> _serviceFileAnalyses = [];
    private FileMultiAnalysisId _multiAnalysisId;
    private ConsumeContext<AnalyzeFile> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumer"/> class.
    /// </summary>
    /// <param name="brokerOptions">The broker settings options.</param>
    /// <param name="logger">The logger instance to log messages.</param>
    /// <param name="options">The options monitor for file analysis consumer options.</param>
    /// <param name="analyzers">The collection of service analyzers to use for file analysis.</param>
    public AnalyzeFileConsumer(
        IOptions<BrokerSettings> brokerOptions,
        ILogger<AnalyzeFileConsumer> logger,
        IOptionsMonitor<AnalyzeFileConsumerSettings> options,
        IEnumerable<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>> analyzers)
    {
        _brokerOptions = brokerOptions;
        _logger = logger;
        _options = options;
        _analyzers = analyzers;

        var uriBuilder = new UriBuilder
        {
            Scheme = "rabbitmq",
            Host = _brokerOptions.Value.Host,
            Port = _brokerOptions.Value.Port,
            Path = UpdateFileMultiAnalysisConsumer.EndpointName,
        };
        _updateAnalysisEndpointUri = uriBuilder.Uri;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<AnalyzeFile> context)
    {
        _context = context;
        _multiAnalysisId = context.Message.FileMultiAnalysisId;

        await AnalyzeAsync(context.CancellationToken);
        await UpdateAnalysisStatusAsync(context.CancellationToken);
        await SendUpdateAsync();
    }

    /// <summary>
    /// Analyzes the file using the available analyzers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task AnalyzeAsync(CancellationToken cancellationToken)
    {
        var fileStreamOptions = new FileStreamOptions
        {
            Access = FileAccess.Read,
            Options = FileOptions.Asynchronous | FileOptions.DeleteOnClose,
            Mode = FileMode.Open,
        };
        await using var fileStream = new FileStream(
            _context.Message.TempFilePath,
            fileStreamOptions);

        var request = new FileAnalysisRequest(
            fileStream,
            _context.Message.FileName,
            _context.Message.FileContentType,
            _context.Message.FileDescription,
            _context.Message.FilePassword,
            _context.Message.IsPrivateFile);

        await Parallel.ForEachAsync(_analyzers, cancellationToken, async (analyzer, ct) =>
        {
            fileStream.Position = 0;
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

            ServiceFileAnalysis analysis = analysisResult.Value;
            _serviceFileAnalyses.Add(analysis.Id, analysis);
        });

        await SendUpdateAsync();
    }

    /// <summary>
    /// Updates the analysis asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateAnalysisStatusAsync(CancellationToken cancellationToken)
    {
        Func<ServiceFileAnalysis, bool> analysisFinished = s =>
            s.Status is AnalysisStatus.Finished or AnalysisStatus.Timeout;
        while (!cancellationToken.IsCancellationRequested && !_serviceFileAnalyses.Values.All(analysisFinished))
        {
            await RunRequestsBatchAsync(cancellationToken);
            await SendUpdateAsync();
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
        for (int i = 0; i < _options.CurrentValue.MaxRequestsPerBatch; i++)
        {
            await RunBatchCycleAsync(cancellationToken);
            await Task.Delay(_options.CurrentValue.RequestFrequencyMs, cancellationToken);
        }
    }

    /// <summary>
    /// Executes an HTTP request for each <see cref="ServiceFileAnalysis"/> to update them.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RunBatchCycleAsync(CancellationToken cancellationToken)
    {
        var analyzersMap = _analyzers.ToDictionary(a => a.ServiceName);
        await Parallel.ForEachAsync(
            _serviceFileAnalyses.Values,
            cancellationToken,
            async (analysis, token) =>
            {
                var analyzer = analyzersMap[analysis.ServiceName];
                var result = await analyzer.GetAnalysisAsync(analysis.Id, token);
                if (result.IsError)
                {
                    _logger.LogError("One or more errors occurred while updating file service analysis {Error}", result.Errors);
                    return;
                }

                ServiceFileAnalysis updatedAnalysis = result.Value;
                _serviceFileAnalyses[updatedAnalysis.Id] = updatedAnalysis;
            });
    }

    /// <summary>
    /// Sends a request to update a <see cref="FileMultiAnalysis"/> using updated <see cref="ServiceFileAnalysis"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SendUpdateAsync()
    {
        var request = new UpdateFileMultiAnalysis.UpdateFileMultiAnalysis(
            _multiAnalysisId,
            _serviceFileAnalyses.Values.ToArray());
        await _context.Send(_updateAnalysisEndpointUri, request);
    }
}