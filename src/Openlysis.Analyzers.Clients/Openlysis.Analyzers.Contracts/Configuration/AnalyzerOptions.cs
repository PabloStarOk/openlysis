namespace Openlysis.Analyzers.Contracts.Configuration;

/// <summary>
/// Represents the options for configuring the analyzer.
/// </summary>
/// <param name="ServiceName">The name of the service.</param>
/// <param name="ApiKeyHeaderName">The name of the API key header.</param>
/// <param name="RequestsTimeoutMs">The timeout for requests in milliseconds.</param>
public record AnalyzerOptions(
    string ServiceName,
    string ApiKeyHeaderName,
    int RequestsTimeoutMs);