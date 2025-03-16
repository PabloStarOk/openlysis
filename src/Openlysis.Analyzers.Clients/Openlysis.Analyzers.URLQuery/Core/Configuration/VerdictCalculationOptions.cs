using Openlysis.Analyzers.URLQuery.Core.Models.Objects;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.URLQuery.Core.Configuration;

/// <summary>
/// Options to calculate <see cref="Verdict"/> from <see cref="Stats"/>.
/// </summary>
public record VerdictCalculationOptions
{
    /// <summary>
    /// The configuration section name for VerdictCalculationOptions.
    /// </summary>
    public const string SectionName = "UrlQuery:VerdictCalculation";

    /// <summary>
    /// Gets the number of urlquery alerts to consider a URL as suspicious.
    /// </summary>
    public int UqSuspiciousThreshold { get; init; }

    /// <summary>
    /// Gets the number of urlquery alerts to consider a URL as malware.
    /// </summary>
    public int UqMaliciousThreshold { get; init; }

    /// <summary>
    /// Gets the number of IDS alerts to consider a URL as suspicious.
    /// </summary>
    public int IdsSuspiciousThreshold { get; init; }

    /// <summary>
    /// Gets the number of IDS alerts to consider a URL as malware.
    /// </summary>
    public int IdsMaliciousThreshold { get; init; }

    /// <summary>
    /// Gets the number of TDS alerts to consider a URL as suspicious.
    /// </summary>
    public int TdsSuspiciousThreshold { get; init; }

    /// <summary>
    /// Gets the number of TDS alerts to consider a URL as malware.
    /// </summary>
    public int TdsMaliciousThreshold { get; init; }
}