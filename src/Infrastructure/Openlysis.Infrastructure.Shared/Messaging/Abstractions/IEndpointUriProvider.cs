namespace Openlysis.Infrastructure.Shared.Messaging.Abstractions;

/// <summary>
/// Defines a provider of URIs of message broker endpoints.
/// </summary>
public interface IEndpointUriProvider
{
    /// <summary>
    /// Gets the URI for analyzing a file.
    /// </summary>
    public Uri AnalyzeFileUri { get; }

    /// <summary>
    /// Gets the URI for updating a multi-analysis.
    /// </summary>
    public Uri UpdateMultiAnalysisUri { get; }

    /// <summary>
    /// Gets the URI for analyzing a URL.
    /// </summary>
    public Uri AnalyzeUrlUri { get; }

    /// <summary>
    /// Gets the URI for updating a URL multi-analysis.
    /// </summary>
    public Uri UpdateUrlMultiAnalysisUri { get; }
}