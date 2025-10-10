using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Openlysis.Analyzers.VirusTotal.Core.Abstractions;
using Openlysis.Analyzers.VirusTotal.Core.Configuration;
using Openlysis.Analyzers.VirusTotal.Core.Models.Objects;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.VirusTotal.Infrastructure.Calculations;

/// <summary>
/// Represents a calculator for determining verdicts based on alert statistics.
/// </summary>
public class VerdictCalculator : IVerdictCalculator
{
    private readonly IOptionsMonitor<VerdictCalculationOptions> _options;
    private readonly ILogger<VerdictCalculator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerdictCalculator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="options">The options monitor for verdict calculation settings.</param>
    public VerdictCalculator(
        ILogger<VerdictCalculator> logger,
        IOptionsMonitor<VerdictCalculationOptions> options)
    {
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Verdict Calculate(Stats detectionStats)
    {
        VerdictCalculationOptions options = _options.CurrentValue;
        int totalDetections = detectionStats.GetTotal();

        if (totalDetections is 0)
        {
            _logger.LogWarning("Number of detection stats of VirusTotal was 0.");
            return Verdict.Unknown;
        }

        float timeoutPercentage = GetPercentage(detectionStats.Timeout, totalDetections);
        if (timeoutPercentage > options.MaxAllowedTimeoutPercentage)
        {
            return Verdict.Unknown;
        }

        float maliciousPercentage = GetPercentage(detectionStats.Malicious, totalDetections);
        if (maliciousPercentage >= options.MaliciousThresholdPercentage)
        {
            return Verdict.Malicious;
        }

        float suspiciousPercentage = GetPercentage(detectionStats.Suspicious, totalDetections);
        if (suspiciousPercentage >= options.SuspiciousThresholdPercentage)
        {
            return Verdict.Suspicious;
        }

        float undetectedPercentage = GetPercentage(
            detectionStats.Harmless + detectionStats.Undetected,
            totalDetections);
        return undetectedPercentage >= options.UndetectedThresholdPercentage ? Verdict.Undetected : Verdict.Unknown;
    }

    /// <summary>
    /// Calculates the percentage of a value relative to a total.
    /// </summary>
    /// <param name="value">The part value to calculate the percentage for.</param>
    /// <param name="total">The total value to calculate the percentage against.</param>
    /// <returns>The percentage of the value relative to the total.</returns>
    private static float GetPercentage(int value, int total)
    {
        return (value * 100f) / total;
    }
}