using System.ComponentModel.DataAnnotations;

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
    /// Gets the base address of the service.
    /// </summary>
    [Url]
    required public Uri BaseAddress { get; init; }

    /// <summary>
    /// Gets the name of the API key header.
    /// </summary>
    required public string ApiKeyHeaderName { get; init;  }

    /// <summary>
    /// Gets the timeout for requests in milliseconds.
    /// </summary>
    required public int RequestsTimeoutMs { get; init; }

    /// <summary>
    /// Gets a value indicating whether to analyze consume requests.
    /// </summary>
    [AllowedValues(true, false)]
    required public bool AnalyzeConsumeRequest { get; init; }

    /// <summary>
    /// Gets a value indicating whether to get status consume requests.
    /// </summary>
    [AllowedValues(true, false)]
    required public bool GetStatusConsumeRequest { get; init; }

    /// <summary>
    /// Gets a value indicating whether to get analysis consume requests.
    /// </summary>
    [AllowedValues(true, false)]
    required public bool GetAnalysisConsumeRequest { get; init; }
}