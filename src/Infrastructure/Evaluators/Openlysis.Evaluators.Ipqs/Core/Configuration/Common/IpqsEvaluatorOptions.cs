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
}