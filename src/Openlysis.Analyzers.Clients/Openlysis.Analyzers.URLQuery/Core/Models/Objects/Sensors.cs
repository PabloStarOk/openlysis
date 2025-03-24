#if DEBUG
using System.Text;
#endif
using System.Text.Json.Serialization;

namespace Openlysis.Analyzers.URLQuery.Core.Models.Objects;

/// <summary>
/// Represents a set of sensors which their respective alerts.
/// </summary>
/// <param name="IdsSensors">A <see cref="Array"/> of <see cref="IdsSensor"/>.</param>
/// <param name="TdsSensors">A <see cref="Array"/> of <see cref="TdsSensor"/>.</param>
/// <param name="UrlQueryAlerts">A <see cref="Array"/> of <see cref="AnalyzerAlert"/>.</param>
public record Sensors(
    [property: JsonPropertyName("ids")] IdsSensor[]? IdsSensors,
    [property: JsonPropertyName("analyzer")] TdsSensor[]? TdsSensors,
    [property: JsonPropertyName("urlquery")] AnalyzerAlert[]? UrlQueryAlerts)
{
#if DEBUG
    /// <inheritdoc/>
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        if (IdsSensors is not null)
        {
            stringBuilder.AppendLine("Intrusion Detection Systems Sensors:");

            foreach (var alerts in IdsSensors.Select(s => s.Alerts).ToArray())
            {
                if (alerts is null)
                {
                    continue;
                }

                foreach (var alert in alerts)
                {
                    stringBuilder.AppendLine($"\tAlert: {{ Severity: {alert.Severity} }}");
                }
            }
        }

        if (TdsSensors is not null)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Threat Detection Systems Sensors:");
            foreach (var alerts in TdsSensors.Select(s => s.Alerts).ToArray())
            {
                if (alerts is null)
                {
                    continue;
                }

                foreach (var alert in alerts)
                {
                    stringBuilder.AppendLine($"\tAlert: {{ Verdict: {alert.Verdict}, Severity: {alert.Severity} }}");
                }
            }
        }

        if (UrlQueryAlerts is null)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("URLQuery Alerts:");
        foreach (var alert in UrlQueryAlerts)
        {
            stringBuilder.AppendLine($"\tURL Query Alert: {{ Verdict: {alert.Verdict}, Severity: {alert.Severity} }}");
        }

        return stringBuilder.ToString();
    }
#endif
}