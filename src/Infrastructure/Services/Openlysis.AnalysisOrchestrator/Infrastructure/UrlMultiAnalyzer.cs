using System.Collections.Generic;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.AnalysisOrchestrator.Abstractions;
using Openlysis.AnalysisOrchestrator.Configuration;
using Openlysis.Analyzers.Shared.Contracts.Common.Abstractions;
using Openlysis.Analyzers.Shared.Contracts.URLs.Requests;
using Openlysis.Domain.URLs.Entities;
using Openlysis.Infrastructure.Shared.Communication.Contracts;

namespace Openlysis.AnalysisOrchestrator.Infrastructure;

/// <summary>
/// Analyzes URLs using multiple analyzers, handling analysis jobs and requests.
/// </summary>
internal sealed class UrlMultiAnalyzer : MultiAnalyzer<UrlAnalysis, UrlAnalysisJobMessage, AnalyzeUrlRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMultiAnalyzer"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging analyzer activity.</param>
    /// <param name="options">Options for configuring the multi-analyzer.</param>
    /// <param name="analyzers">Dictionary of available analyzers keyed by name.</param>
    /// <param name="channel">Channel for processing file analysis results.</param>
    public UrlMultiAnalyzer(
        ILogger<UrlMultiAnalyzer> logger,
        IOptions<MultiAnalyzerOptions> options,
        IReadOnlyDictionary<string, Analyzer<UrlAnalysis, AnalyzeUrlRequest>> analyzers,
        Channel<UrlAnalysis> channel)
        : base(logger, options, analyzers, channel)
    {
    }

    /// <inheritdoc/>
    protected override AnalyzeUrlRequest CreateRequest(UrlAnalysisJobMessage message)
    {
        return new AnalyzeUrlRequest(message.Url);
    }
}