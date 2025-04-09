using Openlysis.Assessors.Shared.Configuration;

namespace Openlysis.Assessors.Ipqs.Core.Configuration.Common;

/// <summary>
/// Represents the configuration options for the IPQualityScore service.
/// </summary>
public record IpqsAssessorOptions : DataAssessorOptions
{
    /// <summary>
    /// The section name in the configuration file.
    /// </summary>
    public const string SectionName = "Ipqs:Assessor";
}