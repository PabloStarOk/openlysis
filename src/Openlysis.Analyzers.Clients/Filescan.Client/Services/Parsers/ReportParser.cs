using System.Text.Json;

using Filescan.Client.Abstractions;

using Openlysis.Domain.Common.Enums;
using Openlysis.Domain.FileAnalyses.Entities;

namespace Filescan.Client.Services.Parsers;

/// <summary>
/// Creates <see cref="Report"/> objects from <see cref="JsonElement"/>.
/// </summary>
public class ReportParser : ModelParser<Report, JsonProperty>
{
    private static readonly Dictionary<string, Verdict> VerdictsMap = new ()
    {
        { "UNKNOWN", Verdict.Unknown },
        { "NOTHREAT", Verdict.Undetected },
        { "BENIGN", Verdict.Undetected },
        { "SUSPICIOUS", Verdict.Suspicious },
        { "LIKELYMALICIOUS", Verdict.Malicious },
        { "MALICIOUS", Verdict.Malicious },
    };

    /// <inheritdoc/>
    public override Report Parse(JsonProperty rootProperty)
    {
        JsonElement element = rootProperty.Value;

        Verdict verdict = Verdict.Unknown;
        float threatLevel = 0;
        if (element.TryGetProperty("finalVerdict", out JsonElement finalVerdictElement))
        {
            verdict = ParseVerdict(finalVerdictElement);
            threatLevel = (float)finalVerdictElement.GetProperty("threatLevel").GetDecimal();
        }

        return Report.Create(
            rootProperty.Name,
            verdict,
            CalculateThreatZone(verdict),
            threatLevel);
    }

    /// <summary>
    /// Parses the verdict from the given JsonElement.
    /// </summary>
    /// <param name="finalVerdictElement">The JsonElement containing the verdict information.</param>
    /// <returns>The parsed <see cref="Verdict"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an invalid verdict string is given.</exception>
    private static Verdict ParseVerdict(JsonElement finalVerdictElement)
    {
        string verdictString = GetStringOrEmpty("verdict", finalVerdictElement);
        verdictString = verdictString
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .ToUpper();
        return VerdictsMap[verdictString];
    }

    /// <summary>
    /// Calculates the threat zone based on the given verdict.
    /// </summary>
    /// <param name="verdict">The verdict to evaluate.</param>
    /// <returns>The corresponding <see cref="ThreatZone"/> for the given verdict.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the verdict is out of the expected range.</exception>
    private static ThreatZone CalculateThreatZone(Verdict verdict)
    {
        return verdict switch
        {
            Verdict.Malicious => ThreatZone.Red,
            Verdict.Suspicious => ThreatZone.Yellow,
            Verdict.Undetected => ThreatZone.Green,
            Verdict.Unknown => ThreatZone.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(verdict), verdict, null)
        };
    }
}