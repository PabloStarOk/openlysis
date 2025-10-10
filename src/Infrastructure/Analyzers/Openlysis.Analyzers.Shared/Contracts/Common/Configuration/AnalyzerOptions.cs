using System.ComponentModel.DataAnnotations;

using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.Shared.Contracts.Common.Configuration;

/// <summary>
/// Represents the options for configuring the analyzer.
/// </summary>
public abstract record AnalyzerOptions : ServiceOptions
{
    /// <summary>
    /// Gets the name of the API key header.
    /// </summary>
    [Required]
    [MinLength(1)]
    required public string ApiKeyHeaderName { get; init;  }
}