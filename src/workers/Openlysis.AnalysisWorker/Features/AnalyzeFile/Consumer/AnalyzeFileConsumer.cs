using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.AnalysisWorker.Core.Abstractions;
using Openlysis.AnalysisWorker.Features.AnalyzeFile.Contracts;
using Openlysis.AnalysisWorker.Infrastructure.Configuration;
using Openlysis.Analyzers.Contracts.Interfaces;
using Openlysis.Analyzers.Contracts.Requests;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses;
using Openlysis.Domain.FileAnalyses.Entities;
using Openlysis.Domain.FileAnalyses.ValueObjects;

namespace Openlysis.AnalysisWorker.Features.AnalyzeFile.Consumer;

/// <summary>
/// Consumer class for handling file analysis job requests.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IConsumer{T}"/> interface to process <see cref="AnalyzeFile"/> messages.
/// </remarks>
public class AnalyzeFileConsumer : IConsumer<Contracts.AnalyzeFile>
{
    /// <summary>
    /// Name of the endpoint.
    /// </summary>
    public const string EndpointName = "analyze-file";

    private readonly ILogger<AnalyzeFileConsumer> _logger;
    private readonly IOptionsMonitor<AnalyzeFileConsumerSettings> _options;
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly IEnumerable<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>> _analyzers;
    private readonly Dictionary<ServiceFileAnalysisId, ServiceFileAnalysis> _serviceFileAnalyses = [];
    private readonly Func<ServiceFileAnalysis, bool> _analysisFinished = s =>
        s.Status is AnalysisStatus.Finished or AnalysisStatus.Timeout;

    private FileMultiAnalysisId _multiAnalysisId;
    private ConsumeContext<Contracts.AnalyzeFile> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeFileConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log messages.</param>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="options">The options monitor for file analysis consumer options.</param>
    /// <param name="analyzers">The collection of service analyzers to use for file analysis.</param>
    public AnalyzeFileConsumer(
        ILogger<AnalyzeFileConsumer> logger,
        IEndpointUriProvider endpointUriProvider,
        IOptionsMonitor<AnalyzeFileConsumerSettings> options,
        IEnumerable<IServiceAnalyzer<ServiceFileAnalysis, ServiceFileAnalysisId>> analyzers)
    {
        _logger = logger;
        _endpointUriProvider = endpointUriProvider;
        _options = options;
        _analyzers = analyzers;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<Contracts.AnalyzeFile> context)
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
        while (!cancellationToken.IsCancellationRequested
               && !_serviceFileAnalyses.Values.All(_analysisFinished))
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
        for (int i = 0; i < _options.CurrentValue.MaxRequestsPerBatch; i++)
        {
            await RunBatchCycleAsync(cancellationToken);
            if (_serviceFileAnalyses.Values.All(_analysisFinished))
            {
                break;
            }

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
        bool sendUpdate = false;
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

                if (updatedAnalysis.Status != analysis.Status)
                {
                    sendUpdate = true;
                }

                _serviceFileAnalyses[updatedAnalysis.Id] = updatedAnalysis;
            });

        if (sendUpdate)
        {
            await SendUpdateAsync();
        }
    }

    /// <summary>
    /// Sends a request to update a <see cref="FileMultiAnalysis"/> using updated <see cref="ServiceFileAnalysis"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SendUpdateAsync()
    {
        var request = new UpdateFileMultiAnalysis.Contracts.UpdateFileMultiAnalysis(
            _multiAnalysisId,
            _serviceFileAnalyses.Values.ToArray());
        await _context.Send(_endpointUriProvider.UpdateMultiAnalysisUri, request);
    }
}