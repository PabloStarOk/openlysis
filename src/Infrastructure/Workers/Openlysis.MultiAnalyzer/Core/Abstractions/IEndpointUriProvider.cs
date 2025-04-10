using System;

using Openlysis.MultiAnalyzer.Features.AnalyzeFile.Consumer;
using Openlysis.MultiAnalyzer.Features.Repositories.URLs.Contracts;
using Openlysis.MultiAnalyzer.Features.UpdateFileMultiAnalysis.Consumer;
using Openlysis.MultiAnalyzer.Features.URLs.Analyze.Consumer;

namespace Openlysis.MultiAnalyzer.Core.Abstractions;

/// <summary>
/// Defines a provider of URIs of message broker endpoints.
/// </summary>
public interface IEndpointUriProvider
{
    /// <summary>
    /// Gets the URI of the endpoint for the <see cref="AnalyzeFileConsumer"/> message consumer.
    /// </summary>
    public Uri AnalyzeFileUri { get; }

    /// <summary>
    /// Gets the URI of the endpoint for the <see cref="UpdateFileMultiAnalysisConsumer"/> message consumer.
    /// </summary>
    public Uri UpdateMultiAnalysisUri { get; }

    /// <summary>
    /// Gets the URI of the endpoint for the <see cref="AnalyzeUrlConsumer"/> message consumer.
    /// </summary>
    public Uri AnalyzeUrlUri { get; }

    /// <summary>
    /// Gets the URI of the endpoint for the <see cref="UpdateUrlMultiAnalysis"/> message consumer.
    /// </summary>
    public Uri UpdateUrlMultiAnalysisUri { get; }
}