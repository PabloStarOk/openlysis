using Microsoft.Extensions.Options;

using Openlysis.Analyzers.URLQuery.Core.Abstractions;
using Openlysis.Analyzers.URLQuery.Core.Configuration;
using Openlysis.Analyzers.URLQuery.Core.Models.Enums;
using Openlysis.Analyzers.URLQuery.Core.Models.Objects;
using Openlysis.Domain.Common.Enums;

namespace Openlysis.Analyzers.URLQuery.Infrastructure.Services;

/// <summary>
/// Calculates a <see cref="Verdict"/> from <see cref="Sensors"/>.
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
    public Verdict Calculate(Sensors sensors)
    {
        VerdictCalculationOptions options = _options.CurrentValue;

        // Get verdicts
        Verdict idsVerdict = GetIdsVerdict(sensors.IdsSensors ?? []);
        Verdict tdsVerdict = GetTdsVerdict(sensors.TdsSensors ?? []);
        Verdict urlqueryVerdict = GetUrlqueryVerdict(sensors.UrlQueryAlerts ?? []);

        // Populate according to weight.
        var verdicts = new List<Verdict>();
        for (int i = 0; i < options.SensorWeights.Ids; i++)
        {
            verdicts.Add(idsVerdict);
        }

        for (int i = 0; i < options.SensorWeights.Tds; i++)
        {
            verdicts.Add(tdsVerdict);
        }

        for (int i = 0; i < options.SensorWeights.Urlquery; i++)
        {
            verdicts.Add(urlqueryVerdict);
        }

        // Calculate verdict.
        int maliciousVerdictsCount = verdicts.Count(v => v is Verdict.Malicious);
        if (maliciousVerdictsCount >= options.MaliciousVerdictCountThreshold)
        {
            return Verdict.Malicious;
        }

        int suspiciousVerdictsCount = verdicts.Count(v => v is Verdict.Suspicious);
        return suspiciousVerdictsCount >= options.SuspiciousVerdictCountThreshold
            ? Verdict.Suspicious
            : Verdict.Undetected;
    }

    /// <summary>
    /// Calculates the verdict for IDS sensors.
    /// </summary>
    /// <param name="sensors">Array of IDS sensors.</param>
    /// <returns>The calculated verdict.</returns>
    private Verdict GetIdsVerdict(IdsSensor[] sensors)
    {
        VerdictCalculationOptions options = _options.CurrentValue;

        if (sensors.Length is 0)
        {
            return Verdict.Undetected;
        }

        int maliciousMessagesCount = sensors.Sum(s => CountMaliciousAlertMessages(s.Alerts ?? []));
        int highSeverityCount = sensors.Sum(s => s.Alerts?.Count(a => a.Severity == Severity.High) ?? 0);
        if (highSeverityCount >= options.AlertSeverityThresholds.HighSeverityMaliciousThreshold
            || maliciousMessagesCount >= options.MaliciousAlertMessageThreshold)
        {
            return Verdict.Malicious;
        }

        int suspiciousMessagesCount = sensors.Sum(s => CountSuspiciousAlertMessages(s.Alerts ?? []));
        int mediumSeverityCount = sensors.Sum(s => s.Alerts?.Count(a => a.Severity == Severity.Medium) ?? 0);
        return mediumSeverityCount >= options.AlertSeverityThresholds.MediumSeveritySuspiciousThreshold
        || suspiciousMessagesCount >= options.SuspiciousAlertMessageThreshold
            ? Verdict.Suspicious
            : Verdict.Undetected;
    }

    /// <summary>
    /// Calculates the verdict for TDS sensors.
    /// </summary>
    /// <param name="tdsSensors">Array of TDS sensors.</param>
    /// <returns>The calculated verdict.</returns>
    private Verdict GetTdsVerdict(TdsSensor[] tdsSensors)
    {
        return CalculateAnalyzerAlertsVerdict(
            tdsSensors.SelectMany(s => s.Alerts ?? []).ToArray(),
            _options.CurrentValue.TdsAlertThresholds);
    }

    /// <summary>
    /// Calculates the verdict for URL query alerts.
    /// </summary>
    /// <param name="urlqueryAlerts">Array of URL query alerts.</param>
    /// <returns>The calculated verdict.</returns>
    private Verdict GetUrlqueryVerdict(AnalyzerAlert[] urlqueryAlerts)
    {
        return CalculateAnalyzerAlertsVerdict(
            urlqueryAlerts,
            _options.CurrentValue.UrlqueryAlertThresholds);
    }

    /// <summary>
    /// Counts the number of malicious alert messages.
    /// </summary>
    /// <param name="alerts">Array of alerts.</param>
    /// <returns>The count of malicious alert messages.</returns>
    private int CountMaliciousAlertMessages(Alert[] alerts)
    {
        string[] maliciousMessages = _options.CurrentValue.AlertMessages.Malicious;
        return alerts
            .Count(a => maliciousMessages
                .Contains(VerdictCalculationOptions.NormalizeAlertString(a.Message)));
    }

    /// <summary>
    /// Counts the number of suspicious alert messages.
    /// </summary>
    /// <param name="alerts">Array of alerts.</param>
    /// <returns>The count of suspicious alert messages.</returns>
    private int CountSuspiciousAlertMessages(Alert[] alerts)
    {
        string[] suspiciousMessages = _options.CurrentValue.AlertMessages.Suspicious;

        return alerts
            .Count(a => suspiciousMessages
                .Contains(VerdictCalculationOptions.NormalizeAlertString(a.Message)));
    }

    /// <summary>
    /// Calculates the verdict for analyzer alerts based on thresholds.
    /// </summary>
    /// <param name="alerts">Array of analyzer alerts.</param>
    /// <param name="thresholdsOptions">Threshold options for alert counts.</param>
    /// <returns>The calculated verdict.</returns>
    private Verdict CalculateAnalyzerAlertsVerdict(
        AnalyzerAlert[] alerts,
        AlertCountThresholdsOptions thresholdsOptions)
    {
        if (alerts.Length is 0)
        {
            return Verdict.Undetected;
        }

        Verdict[] verdicts = alerts
            .Select(CalculateAnalyzerAlertVerdict)
            .ToArray();

        int maliciousVerdictsCount = verdicts.Count(v => v is Verdict.Malicious);
        if (maliciousVerdictsCount >= thresholdsOptions.MaliciousAlertThreshold)
        {
            return Verdict.Malicious;
        }

        int suspiciousVerdictsCount = verdicts.Count(v => v is Verdict.Suspicious);
        return suspiciousVerdictsCount >= thresholdsOptions.SuspiciousAlertThreshold
            ? Verdict.Suspicious
            : Verdict.Undetected;
    }

    /// <summary>
    /// Calculates the verdict for a single analyzer alert.
    /// </summary>
    /// <param name="alert">The analyzer alert.</param>
    /// <returns>The calculated verdict.</returns>
    private Verdict CalculateAnalyzerAlertVerdict(AnalyzerAlert alert)
    {
        VerdictCalculationOptions options = _options.CurrentValue;
        string normalizedVerdict = VerdictCalculationOptions.NormalizeAlertString(alert.Verdict);
        string normalizedMessage = VerdictCalculationOptions.NormalizeAlertString(alert.Message);

        if (options.AlertVerdicts.Malicious.Contains(normalizedVerdict)
            || options.AlertMessages.Malicious.Contains(normalizedMessage))
        {
            return Verdict.Malicious;
        }

        if (options.AlertVerdicts.Suspicious.Contains(normalizedVerdict)
            || options.AlertMessages.Suspicious.Contains(normalizedMessage))
        {
            return alert.Severity is Severity.High
                ? Verdict.Malicious
                : Verdict.Suspicious;
        }

        return options.AlertVerdicts.Undetected.Contains(normalizedVerdict)
            ? Verdict.Undetected
            : Verdict.Unknown;
    }
}