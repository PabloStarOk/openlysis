using Openlysis.Evaluators.Shared.Contracts.Configuration;

namespace Openlysis.Evaluators.Ipqs.Core.Configuration.Common;

/// <summary>
/// Represents the configuration options for the IPQualityScore service.
/// </summary>
public record IpqsEvaluatorOptions : ReputationEvaluatorOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "Ipqs:Evaluator";
}