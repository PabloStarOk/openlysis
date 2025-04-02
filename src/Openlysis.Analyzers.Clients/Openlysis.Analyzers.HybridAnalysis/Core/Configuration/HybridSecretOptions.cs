using Openlysis.Analyzers.Shared.Core.Configuration;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Configuration;

/// <summary>
/// Represents the options for Hybrid Analysis secret configuration.
/// </summary>
/// <param name="ApiKey">The API key for Hybrid Analysis.</param>
public record HybridSecretOptions(string ApiKey)
    : SecretOptions(ApiKey)
{
    /// <summary>
    /// The section name for Hybrid Analysis configuration.
    /// </summary>
    public const string SectionName = "HybridAnalysis";
}