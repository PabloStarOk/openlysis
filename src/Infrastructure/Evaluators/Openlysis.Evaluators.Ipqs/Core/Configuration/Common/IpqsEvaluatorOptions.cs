using Openlysis.Infrastructure.Shared.Contracts.Common.Configuration;

namespace Openlysis.Evaluators.Ipqs.Core.Configuration.Common;

/// <summary>
/// Represents the configuration options for the IPQualityScore service.
/// </summary>
public record IpqsEvaluatorOptions : ServiceOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "Ipqs:Evaluator";

    /// <summary>
    /// Gets the name of the secret storing the API key for IPQualityScore.
    /// </summary>
    required public string ApiKeySecretName { get; init; }
}