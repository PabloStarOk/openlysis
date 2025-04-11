using System;

using Openlysis.MultiAnalyzer.Adapters.Broker.Files;
using Openlysis.MultiAnalyzer.Adapters.Broker.URLs;
using Openlysis.MultiAnalyzer.Core.Broker.Files;
using Openlysis.MultiAnalyzer.Core.Broker.URLs;

namespace Openlysis.MultiAnalyzer.Common.Abstractions;

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