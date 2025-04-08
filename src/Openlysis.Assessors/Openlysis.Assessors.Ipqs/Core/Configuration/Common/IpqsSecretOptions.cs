using Openlysis.Infrastructure.Shared.Configuration;

namespace Openlysis.Assessors.Ipqs.Core.Configuration.Common;

/// <summary>
/// Represents the options for IPQualityScore secret configuration.
/// </summary>
public record IpqsSecretOptions : SecretOptions
{
    /// <summary>
    /// The section name for IPQualityScore configuration in the settings file.
    /// </summary>
    public const string SectionName = "Ipqs";
}