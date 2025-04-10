using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ErrorOr;

using MassTransit;

using Microsoft.Extensions.Options;

using Openlysis.Analyzers.Shared.Core.Common.Abstractions;
using Openlysis.Analyzers.Shared.Core.URLs.Requests;
using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.Common.ValueObjects;
using Openlysis.Domain.URLs.Entities;
using Openlysis.MultiAnalyzer.Core.Abstractions;
using Openlysis.MultiAnalyzer.Features.Repositories.URLs.Contracts;
using Openlysis.MultiAnalyzer.Features.URLs.Analyze.Contracts;
using Openlysis.MultiAnalyzer.Infrastructure.Configuration;

namespace Openlysis.MultiAnalyzer.Features.URLs.Analyze.Consumer;

/// <summary>
/// Consumer class for handling AnalyzeUrl messages.
/// </summary>
/// <remarks>
/// This class consumes AnalyzeUrl messages and processes them by analyzing URLs
/// and updating the analysis status.
/// </remarks>
public class AnalyzeUrlConsumer : IConsumer<AnalyzeUrl>
{
    /// <summary>
    /// Name of the endpoint.
    /// </summary>
    public const string EndpointName = "analyze-url";

    private readonly IOptionsMonitor<AnalyzeConsumerOptions> _options;
    private readonly IEndpointUriProvider _endpointUriProvider;
    private readonly Dictionary<string, Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>> _analyzers;
    private readonly Dictionary<ComposedServiceAnalysisId, UrlServiceAnalysis> _serviceAnalyses = [];
    private ConsumeContext<AnalyzeUrl> _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeUrlConsumer"/> class.
    /// </summary>
    /// <param name="options">The options monitor for consumer configuration.</param>
    /// <param name="endpointUriProvider">The provider for endpoint URIs.</param>
    /// <param name="analyzers">The collection of analyzers for URL service analysis.</param>
    public AnalyzeUrlConsumer(
        IOptionsMonitor<AnalyzeConsumerOptions> options,
        IEndpointUriProvider endpointUriProvider,
        IEnumerable<Analyzer<UrlServiceAnalysis, AnalyzeUrlRequest>> analyzers)
    {
        _options = options;
        _endpointUriProvider = endpointUriProvider;
        _analyzers = analyzers.ToDictionary(a => a.ServiceName);
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<AnalyzeUrl> context)
    {
        _context = context;
        await AnalyzeAsync(context.CancellationToken);
        await UpdateAnalysesAsync(context.CancellationToken);
    }

    /// <summary>
    /// Analyzes the URL asynchronously using the available analyzers.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task AnalyzeAsync(CancellationToken cancellationToken)
    {
        var request = new AnalyzeUrlRequest(_context.Message.Url);
        await Parallel.ForEachAsync(_analyzers.Values, cancellationToken, async (analyzer, ct) =>
        {
            if (!analyzer.CanAnalyze)
            {
                return;
            }

            ErrorOr<UrlServiceAnalysis> result = await analyzer.AnalyzeAsync(request, ct);
            if (result.IsError)
            {
                return;
            }

            _serviceAnalyses.Add(result.Value.Id, result.Value);
        });
        await SendUpdateAsync(_serviceAnalyses.Values.ToArray());
    }

    /// <summary>
    /// Updates the analyses by executing batches of URL service analyses asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task UpdateAnalysesAsync(CancellationToken cancellationToken)
    {
        while (_serviceAnalyses.Count > 0)
        {
            await ExecuteBatchAsync(cancellationToken);
            await Task.Delay(_options.CurrentValue.RequestBatchWaitTimeMs, cancellationToken);
        }
    }

    /// <summary>
    /// Executes a batch of URL service analyses asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task ExecuteBatchAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < _options.CurrentValue.RequestsPerBatch; i++)
        {
            await ExecuteBatchCycleAsync(cancellationToken);
            if (_serviceAnalyses.Count is 0)
            {
                break;
            }

            await Task.Delay(_options.CurrentValue.RequestFrequencyMs, cancellationToken);
        }
    }

    /// <summary>
    /// Executes a batch cycle to process URL service analyses.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task ExecuteBatchCycleAsync(CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(_serviceAnalyses.Values, cancellationToken, async (analysis, ct) =>
        {
            var analyzer = _analyzers[analysis.ServiceName];
            if (!analyzer.CanGetAnalysisStatus)
            {
                return;
            }

            // Get status
            ErrorOr<AnalysisStatus> getStatusResult = await analyzer.GetStatusAsync(analysis.Id, cancellationToken);
            if (getStatusResult.IsError)
            {
                analysis.UpdateStatus(AnalysisStatus.Failed);
                _serviceAnalyses.Remove(analysis.Id);
                await SendUpdateAsync(analysis);
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
                return;
            }

            // Get full analysis
            ErrorOr<UrlServiceAnalysis> getAnalysisResult = await analyzer.GetAnalysisAsync(analysis.Id, ct);
            if (getStatusResult.IsError)
            {
                return;
            }

            analysis = getAnalysisResult.Value;
            _serviceAnalyses.Remove(analysis.Id);
            await SendUpdateAsync(analysis);
        });
    }

    /// <summary>
    /// Sends an update for the given URL service analysis to the main application.
    /// </summary>
    /// <param name="analyses">The URL service analyses to be updated.</param>
    private async Task SendUpdateAsync(
        params UrlServiceAnalysis[] analyses)
    {
        var request = new UpdateUrlMultiAnalysis(
            _context.Message.MultiAnalysisId,
            analyses);
        await _context.Send(_endpointUriProvider.UpdateUrlMultiAnalysisUri, request);
    }
}