namespace Openlysis.Analyzers.Contracts.Configuration;

/// <summary>
/// Represents the settings for the analyzer.
/// </summary>
public record AnalyzerSettings
{
    /// <summary>
    /// Gets the API key for authentication.
    /// </summary>
    required public string ApiKey { get; init; }

    /// <summary>
    /// Gets the timeout for analysis requests in milliseconds.
    /// </summary>
    required public int RequestsTimeoutMs { get; init; }
}