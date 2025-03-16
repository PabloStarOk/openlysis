using Microsoft.Extensions.Options;

using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Models.Objects;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.URLQuery.Infrastructure.Services;

/// <summary>
/// Calculates a <see cref="Verdict"/> from <see cref="Stats"/>.
/// </summary>
public class VerdictCalculator : IVerdictCalculator
{
    private readonly IOptionsMonitor<VerdictCalculationOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerdictCalculator"/> class.
    /// </summary>
    /// <param name="options">The options monitor for <see cref="VerdictCalculationOptions"/>.</param>
    public VerdictCalculator(IOptionsMonitor<VerdictCalculationOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public Verdict Calculate(Stats alertStats)
    {
        VerdictCalculationOptions options = _options.CurrentValue;

        int uqAlerts = alertStats.UrlQueryAlerts;
        int idsAlerts = alertStats.IdsAlerts;
        int tdsAlerts = alertStats.ThreatDetectionSystemsAlerts;

        Verdict uqVerdict = GetVerdict(
            uqAlerts,
            options.UqSuspiciousThreshold,
            options.UqMaliciousThreshold);

        Verdict idsVerdict = GetVerdict(
            idsAlerts,
            options.IdsSuspiciousThreshold,
            options.IdsMaliciousThreshold);

        Verdict tdsVerdict = GetVerdict(
            tdsAlerts,
            options.TdsSuspiciousThreshold,
            options.TdsMaliciousThreshold);

        Verdict[] verdicts = [uqVerdict, idsVerdict, tdsVerdict];
        return (Verdict)((int)verdicts.Average(v => (int)v));
    }

    /// <summary>
    /// Determines the verdict based on the number of alerts and specified thresholds.
    /// </summary>
    /// <param name="alerts">The number of alerts.</param>
    /// <param name="suspiciousThreshold">The threshold for a suspicious verdict.</param>
    /// <param name="maliciousThreshold">The threshold for a malicious verdict.</param>
    /// <returns>
    /// A <see cref="Verdict"/> value indicating the result of the calculation.
    /// </returns>
    private static Verdict GetVerdict(
        int alerts,
        int suspiciousThreshold,
        int maliciousThreshold)
    {
        if (alerts >= suspiciousThreshold)
        {
            return Verdict.Suspicious;
        }

        return alerts >= maliciousThreshold ? Verdict.Malicious : Verdict.Undetected;
    }
}