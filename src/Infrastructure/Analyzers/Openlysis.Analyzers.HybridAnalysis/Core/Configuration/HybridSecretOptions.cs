using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Analyzers.HybridAnalysis.Core.Configuration;

/// <summary>
/// Represents the options for Hybrid Analysis secret configuration.
/// </summary>
public record HybridSecretOptions
    : SecretOptions
{
    /// <summary>
    /// The section name for Hybrid Analysis configuration.
    /// </summary>
    public const string SectionName = "HybridAnalysis";
}