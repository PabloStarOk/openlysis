namespace Openlysis.Analyzers.Contracts.Core.Configuration;

/// <summary>
/// Represents the options for configuring the analyzer.
/// </summary>
public record AnalyzerOptions
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    required public string ServiceName { get; init; }
    
    /// <summary>
    /// Gets the name of the API key header.
    /// </summary>
    required public string ApiKeyHeaderName { get; init;  }
    
    /// <summary>
    /// Gets the timeout for requests in milliseconds.
    /// </summary>
    required public int RequestsTimeoutMs { get; init; }
}