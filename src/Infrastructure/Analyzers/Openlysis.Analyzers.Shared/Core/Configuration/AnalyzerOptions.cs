using Openlysis.Infrastructure.Shared.Configuration;

namespace Openlysis.Analyzers.Shared.Core.Configuration;

/// <summary>
/// Represents the options for configuring the analyzer.
/// </summary>
public abstract record AnalyzerOptions : ServiceOptions
{
    /// <summary>
    /// Gets the name of the API key header.
    /// </summary>
    required public string ApiKeyHeaderName { get; init;  }
}