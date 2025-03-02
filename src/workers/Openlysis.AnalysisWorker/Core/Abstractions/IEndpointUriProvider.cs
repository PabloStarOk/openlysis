using System;

using Openlysis.AnalysisWorker.Features.AnalyzeFile.Consumer;
using Openlysis.AnalysisWorker.Features.UpdateFileMultiAnalysis.Consumer;

namespace Openlysis.AnalysisWorker.Core.Abstractions;

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
}